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

        internal struct Ctx
        {
            internal VideoPlayer VideoPlayer;
            internal Action<RenderTexture> SetTexture;
            internal CancellationToken CancellationToken;
            internal Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;
        private RenderTexture _renderTexture;
        private UniTaskCompletionSource _prepared;
        private UniTaskCompletionSource _completed;
        private string _error;

        internal VideoPlayback(Ctx ctx)
        {
            _ctx = ctx;
            _ctx.VideoPlayer.prepareCompleted += OnPrepared;
            _ctx.VideoPlayer.loopPointReached += OnCompleted;
            _ctx.VideoPlayer.errorReceived += OnFailed;
        }

        internal async UniTask<VideoPlaybackStatus> Play(VideoPlaybackRequest request)
        {
            Stop();

            _prepared = new UniTaskCompletionSource();
            _completed = new UniTaskCompletionSource();
            _error = null;

            _renderTexture = new RenderTexture(
                request.Width,
                request.Height,
                16,
                RenderTextureFormat.ARGB32);
            _renderTexture.Create();
            _ctx.SetTexture(_renderTexture);

            var videoPlayer = _ctx.VideoPlayer;
            videoPlayer.url = request.Url;
            videoPlayer.isLooping = request.Loop;
            videoPlayer.playbackSpeed = request.Speed;
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

            videoPlayer.Play();
            return VideoPlaybackStatus.Ready;
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

        private void OnCompleted(VideoPlayer source)
        {
            _completed?.TrySetResult();
        }

        private void OnFailed(VideoPlayer source, string message)
        {
            _error = message;
            LogFailure(source.url);
            _prepared?.TrySetResult();
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
            _ctx.VideoPlayer.loopPointReached -= OnCompleted;
            _ctx.VideoPlayer.errorReceived -= OnFailed;
            Stop();
        }
    }
}
