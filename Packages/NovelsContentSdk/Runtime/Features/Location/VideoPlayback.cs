using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Video;

namespace Novels.Location
{
    internal enum VideoPlaybackStatus
    {
        Ready,
        Completed,
        Failed,
    }

    internal readonly struct VideoPlaybackRequest
    {
        internal readonly string Url;
        internal readonly int Width;
        internal readonly int Height;
        internal readonly bool Loop;
        internal readonly float Speed;

        internal VideoPlaybackRequest(
            string url,
            int width,
            int height,
            bool loop,
            float speed)
        {
            Url = url;
            Width = width;
            Height = height;
            Loop = loop;
            Speed = speed;
        }
    }

    internal sealed class VideoPlayback : BaseDisposable
    {
        private const int _preparationTimeoutMilliseconds = 10000;

        internal struct Dependencies
        {
            internal VideoPlayer VideoPlayer;
            internal Action<RenderTexture> SetTexture;
            internal CancellationToken CancellationToken;
            internal Action<Diagnostics.NovelError> OnError;
        }

        private readonly Dependencies _ctx;
        private RenderTexture _renderTexture;
        private UniTaskCompletionSource _prepared;
        private UniTaskCompletionSource _firstFrame;
        private UniTaskCompletionSource _completed;
        private string _error;
        private bool _acceptFrames;

        internal VideoPlayback(Dependencies ctx)
        {
            _ctx = ctx;
            _ctx.VideoPlayer.prepareCompleted += OnPrepared;
            _ctx.VideoPlayer.frameReady += OnFrameReady;
            _ctx.VideoPlayer.loopPointReached += OnCompleted;
            _ctx.VideoPlayer.errorReceived += OnFailed;
        }

        internal async UniTask<VideoPlaybackStatus> Play(VideoPlaybackRequest request)
        {
            Stop();

            _prepared = new UniTaskCompletionSource();
            _completed = new UniTaskCompletionSource();
            _error = null;

            var videoPlayer = _ctx.VideoPlayer;
            videoPlayer.url = request.Url;
            videoPlayer.isLooping = request.Loop;
            videoPlayer.playbackSpeed = request.Speed;
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.Prepare();

            var timeout = UniTask.Delay(
                _preparationTimeoutMilliseconds,
                cancellationToken: _ctx.CancellationToken);
            var completedTaskIndex = await UniTask.WhenAny(_prepared.Task, timeout);
            _ctx.CancellationToken.ThrowIfCancellationRequested();

            if (completedTaskIndex == 1)
            {
                _error = $"Video preparation timed out after {_preparationTimeoutMilliseconds} ms";
                LogFailure(request.Url);
                Stop();
                return VideoPlaybackStatus.Failed;
            }

            if (_error != null)
            {
                Stop();
                return VideoPlaybackStatus.Failed;
            }

            var renderSize = ResolveRenderSize(videoPlayer, request);
            _renderTexture = new RenderTexture(
                renderSize.x,
                renderSize.y,
                0,
                RenderTextureFormat.ARGB32);
            _renderTexture.Create();
            _ctx.SetTexture(_renderTexture);
            _firstFrame = new UniTaskCompletionSource();
            _acceptFrames = true;
            videoPlayer.Play();

            timeout = UniTask.Delay(
                _preparationTimeoutMilliseconds,
                cancellationToken: _ctx.CancellationToken);
            completedTaskIndex = await UniTask.WhenAny(_firstFrame.Task, timeout);
            _ctx.CancellationToken.ThrowIfCancellationRequested();
            if (completedTaskIndex == 1)
            {
                _error = $"Video first frame timed out after "
                    + $"{_preparationTimeoutMilliseconds} ms";
                LogFailure(request.Url);
                Stop();
                return VideoPlaybackStatus.Failed;
            }
            if (_error != null)
            {
                Stop();
                return VideoPlaybackStatus.Failed;
            }

            // VideoPlayer.frameReady means that the decoder produced a frame,
            // but the target RenderTexture can still contain its initial black
            // contents until Unity's video/render update finishes.  Do not let
            // the UI crossfade to that texture before the render loop has had a
            // chance to publish the decoded frame.
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate,
                _ctx.CancellationToken);
            return VideoPlaybackStatus.Ready;
        }

        private static Vector2Int ResolveRenderSize(
            VideoPlayer videoPlayer,
            VideoPlaybackRequest request)
        {
            var width = videoPlayer.width > 0
                ? (int)Math.Min(videoPlayer.width, (uint)int.MaxValue)
                : request.Width;
            var height = videoPlayer.height > 0
                ? (int)Math.Min(videoPlayer.height, (uint)int.MaxValue)
                : request.Height;
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            var maximum = Math.Max(1, SystemInfo.maxTextureSize);
            var scale = Math.Min(1f, maximum / (float)Math.Max(width, height));
            return new Vector2Int(
                Math.Max(1, Mathf.RoundToInt(width * scale)),
                Math.Max(1, Mathf.RoundToInt(height * scale)));
        }

        internal async UniTask<VideoPlaybackStatus> WaitForCompletion()
        {
            await _completed.Task.AttachExternalCancellation(_ctx.CancellationToken);
            return _error == null
                ? VideoPlaybackStatus.Completed
                : VideoPlaybackStatus.Failed;
        }

        internal void Stop()
        {
            _acceptFrames = false;
            _firstFrame = null;
            if (_ctx.VideoPlayer != null)
            {
                _ctx.VideoPlayer.Stop();
                _ctx.VideoPlayer.targetTexture = null;
                _ctx.SetTexture(null);
            }

            ReleaseRenderTexture();
        }

        private void OnPrepared(VideoPlayer source)
        {
            _prepared?.TrySetResult();
        }

        private void OnFrameReady(VideoPlayer source, long frameIndex)
        {
            if (_acceptFrames)
                _firstFrame?.TrySetResult();
        }

        private void OnCompleted(VideoPlayer source)
        {
            _completed?.TrySetResult();
        }

        private void OnFailed(VideoPlayer source, string message)
        {
            _error = message;
            LogFailure(source.url);
            _prepared?.TrySetResult();
            _firstFrame?.TrySetResult();
            _completed?.TrySetResult();
        }

        private void LogFailure(string url)
        {
            _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                Diagnostics.NovelErrorCodes.VideoPlaybackFailed,
                Diagnostics.NovelErrorSeverity.Recoverable,
                $"Failed to play video '{url}': {_error}"));
        }

        private void ReleaseRenderTexture()
        {
            if (_renderTexture == null)
                return;

            _renderTexture.Release();
            UnityEngine.Object.Destroy(_renderTexture);
            _renderTexture = null;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_ctx.VideoPlayer == null)
            {
                ReleaseRenderTexture();
                return;
            }

            _ctx.VideoPlayer.prepareCompleted -= OnPrepared;
            _ctx.VideoPlayer.frameReady -= OnFrameReady;
            _ctx.VideoPlayer.loopPointReached -= OnCompleted;
            _ctx.VideoPlayer.errorReceived -= OnFailed;
            Stop();
        }
    }
}
