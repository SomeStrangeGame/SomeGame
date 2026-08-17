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
            Set(assetName, presentation, PlaybackMode.Live);

        internal UniTask SetImmediate(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation) =>
            Set(assetName, presentation, PlaybackMode.Immediate);

        private async UniTask Set(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            PlaybackMode mode)
        {
            _ctx.TargetCamera.backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                    ? Color.white
                    : Color.black;
            await Hide(mode);
            _ctx.Screen.ResetCamera();
            _ctx.Screen.ResetEffect();

            var resources = await UniTask.WhenAll(
                _ctx.GetSprite(assetName)
                    .AttachExternalCancellation(_ctx.CancellationToken),
                _ctx.ResolveVideoUrl(assetName)
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var sprite = resources.Item1;
            var url = resources.Item2;
            var plan = BackgroundPresentationPlan.Create(
                assetName,
                presentation,
                !string.IsNullOrEmpty(url));
            _ctx.Screen.SetImage(sprite);
            if (!plan.UsesVideo)
            {
                await ShowStatic(sprite, mode);
                return;
            }

            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"Video background '{plan.AssetName}' requires a poster sprite.");
            }
            var playbackStatus = await _ctx.VideoPlayback.Play(
                new VideoPlaybackRequest(
                    url,
                    sprite.texture.width,
                    sprite.texture.height,
                    !plan.IsCutScene,
                    mode == PlaybackMode.Immediate && plan.IsCutScene
                        ? Time.timeScale * 5f
                        : Time.timeScale));
            var videoReady = playbackStatus == VideoPlaybackStatus.Ready;
            _ctx.Screen.SetEnabledImage(!videoReady);
            _ctx.Screen.SetEnabledVideo(videoReady);
            await Show(mode);
            if (!plan.IsCutScene)
                return;
            if (videoReady)
                playbackStatus = await _ctx.VideoPlayback.WaitForCompletion();
            if (playbackStatus == VideoPlaybackStatus.Failed)
                await WaitForCutSceneFallback(mode);
            if (!plan.KeepsFinalVideoFrame)
                await ReturnToPoster(sprite, mode);
        }

        private async UniTask ShowStatic(Sprite sprite, PlaybackMode mode)
        {
            _ctx.VideoPlayback.Stop();
            _ctx.Screen.SetImage(sprite);
            _ctx.Screen.SetEnabledImage(true);
            _ctx.Screen.SetEnabledVideo(false);
            await Show(mode);
        }

        private async UniTask ReturnToPoster(Sprite sprite, PlaybackMode mode)
        {
            await Hide(mode);
            _ctx.Screen.ResetCamera();
            _ctx.Screen.ResetEffect();
            await ShowStatic(sprite, mode);
        }

        private UniTask Hide(PlaybackMode mode)
        {
            if (mode == PlaybackMode.Live)
                return _ctx.Screen.HideImage(_ctx.CancellationToken);
            _ctx.Screen.HideImageImmediate();
            return UniTask.CompletedTask;
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
    }
}
