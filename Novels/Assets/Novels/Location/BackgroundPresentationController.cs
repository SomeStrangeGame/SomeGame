using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Location
{
    internal sealed class BackgroundPresentationController
    {
        private enum PlaybackMode
        {
            Live,
            Immediate,
        }

        internal struct Ctx
        {
            internal View.Screen Screen;
            internal VideoPlayback VideoPlayback;
            internal Camera TargetCamera;
            internal Func<string, UniTask<Sprite>> GetSprite;
            internal Func<string, UniTask<string>> ResolveVideoUrl;
            internal CancellationToken CancellationToken;
            internal int CutSceneFallbackDelayMilliseconds;
        }

        private readonly Ctx _ctx;

        internal BackgroundPresentationController(Ctx ctx)
        {
            _ctx = ctx;
            if (ctx.Screen == null)
                throw new ArgumentNullException(nameof(ctx.Screen));
            if (ctx.VideoPlayback == null)
                throw new ArgumentNullException(nameof(ctx.VideoPlayback));
            if (ctx.TargetCamera == null)
                throw new ArgumentNullException(nameof(ctx.TargetCamera));
            if (ctx.GetSprite == null)
                throw new ArgumentNullException(nameof(ctx.GetSprite));
            if (ctx.ResolveVideoUrl == null)
                throw new ArgumentNullException(nameof(ctx.ResolveVideoUrl));
            if (ctx.CutSceneFallbackDelayMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ctx.CutSceneFallbackDelayMilliseconds));
            }
        }

        internal UniTask Set(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation) =>
            Set(assetName, presentation, PlaybackMode.Live, false, IsCutScene(presentation));

        internal UniTask SetImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation) =>
            Set(assetName, presentation, PlaybackMode.Immediate, false, IsCutScene(presentation));

        private async UniTask Set(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            PlaybackMode mode,
            bool forceNoVideo,
            bool cutScene)
        {
            _ctx.TargetCamera.backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                    ? Color.white
                    : Color.black;
            if (mode == PlaybackMode.Live)
                await _ctx.Screen.HideImage(_ctx.CancellationToken);
            else
                _ctx.Screen.HideImageImmediate();
            _ctx.Screen.ResetCamera();
            _ctx.Screen.ResetEffect();

            var sprite = await _ctx.GetSprite(assetName)
                .AttachExternalCancellation(_ctx.CancellationToken);
            _ctx.Screen.SetImage(sprite);
            var url = await _ctx.ResolveVideoUrl(assetName);
            if (!forceNoVideo && !string.IsNullOrEmpty(url))
            {
                var playbackStatus = await _ctx.VideoPlayback.Play(
                    new VideoPlaybackRequest(
                        url,
                        sprite.texture.width,
                        sprite.texture.height,
                        !cutScene,
                        mode == PlaybackMode.Immediate && cutScene
                            ? Time.timeScale * 5f
                            : Time.timeScale));
                var videoReady = playbackStatus == VideoPlaybackStatus.Ready;
                _ctx.Screen.SetEnabledImage(!videoReady);
                _ctx.Screen.SetEnabledVideo(videoReady);
                await Show(mode);
                if (!cutScene)
                    return;
                if (videoReady)
                    playbackStatus = await _ctx.VideoPlayback.WaitForCompletion();
                if (playbackStatus == VideoPlaybackStatus.Failed)
                    await WaitForCutSceneFallback(mode);
                if (!presentation.KeepFinalVideoFrame)
                    await Set(assetName, presentation, mode, true, false);
                return;
            }

            _ctx.VideoPlayback.Stop();
            _ctx.Screen.SetEnabledImage(true);
            _ctx.Screen.SetEnabledVideo(false);
            await Show(mode);
        }

        private UniTask Show(PlaybackMode mode)
        {
            if (mode == PlaybackMode.Live)
                return _ctx.Screen.ShowImage(_ctx.CancellationToken);
            _ctx.Screen.ShowImageImmediate();
            return UniTask.CompletedTask;
        }

        private async UniTask WaitForCutSceneFallback(PlaybackMode mode)
        {
            if (mode == PlaybackMode.Live)
            {
                await UniTask.Delay(
                    _ctx.CutSceneFallbackDelayMilliseconds,
                    cancellationToken: _ctx.CancellationToken);
            }
            else
            {
                await UniTask.Yield(_ctx.CancellationToken);
            }
        }

        private static bool IsCutScene(
            StoryContracts.StoryBackgroundPresentation presentation) =>
            presentation.Type == StoryContracts.StoryBackgroundType.CutScene;
    }
}
