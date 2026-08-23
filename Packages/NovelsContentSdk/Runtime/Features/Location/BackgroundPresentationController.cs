using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Location
{
    internal sealed class BackgroundPresentationController
    {
        internal struct Dependencies
        {
            internal View.LocationScreen Screen;
            internal VideoPlayback VideoPlayback;
            internal Camera TargetCamera;
            internal Func<string, UniTask<Sprite>> GetSprite;
            internal Func<string, UniTask<string>> ResolveVideoUrl;
            internal Sprite MissingBackground;
            internal CancellationToken CancellationToken;
            internal int CutSceneFallbackDelayMilliseconds;
        }

        private readonly Dependencies _ctx;

        internal BackgroundPresentationController(Dependencies ctx)
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
            if (ctx.MissingBackground == null)
                throw new ArgumentNullException(nameof(ctx.MissingBackground));
            if (ctx.CutSceneFallbackDelayMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ctx.CutSceneFallbackDelayMilliseconds));
            }
        }

        internal async UniTask Set(
            string assetName,
            StoryContracts.StoryBackgroundPresentation presentation,
            StoryContracts.PresentationMode mode)
        {
            _ctx.TargetCamera.backgroundColor = presentation.BackgroundColor
                == StoryContracts.StoryBackgroundColor.White
                    ? Color.white
                    : Color.black;
            await Hide(mode);
            _ctx.Screen.ResetCamera();
            _ctx.Screen.ResetEffect();

            if (StoryContracts.StoryBackgroundAssets.IsSolidBlack(assetName))
            {
                await ShowSolidColor(mode);
                return;
            }

            var resources = await UniTask.WhenAll(
                _ctx.GetSprite(assetName)
                    .AttachExternalCancellation(_ctx.CancellationToken),
                _ctx.ResolveVideoUrl(assetName)
                    .AttachExternalCancellation(_ctx.CancellationToken));
            var sprite = resources.Item1;
            var url = resources.Item2;
            if (sprite == null)
            {
                await ShowStatic(_ctx.MissingBackground, mode);
                return;
            }
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

            var playbackStatus = await _ctx.VideoPlayback.Play(
                new VideoPlaybackRequest(
                    url,
                    sprite.texture.width,
                    sprite.texture.height,
                    !plan.IsCutScene,
                    mode == StoryContracts.PresentationMode.Immediate && plan.IsCutScene
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

        private async UniTask ShowStatic(
            Sprite sprite,
            StoryContracts.PresentationMode mode)
        {
            _ctx.VideoPlayback.Stop();
            _ctx.Screen.SetImage(sprite);
            _ctx.Screen.SetEnabledImage(true);
            _ctx.Screen.SetEnabledVideo(false);
            await Show(mode);
        }

        private async UniTask ShowSolidColor(StoryContracts.PresentationMode mode)
        {
            _ctx.VideoPlayback.Stop();
            _ctx.Screen.SetImage(null);
            _ctx.Screen.SetEnabledImage(false);
            _ctx.Screen.SetEnabledVideo(false);
            await Show(mode);
        }

        private async UniTask ReturnToPoster(
            Sprite sprite,
            StoryContracts.PresentationMode mode)
        {
            await Hide(mode);
            _ctx.Screen.ResetCamera();
            _ctx.Screen.ResetEffect();
            await ShowStatic(sprite, mode);
        }

        private UniTask Hide(StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
                return _ctx.Screen.HideImage(_ctx.CancellationToken);
            _ctx.Screen.HideImageImmediate();
            return UniTask.CompletedTask;
        }

        private UniTask Show(StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
                return _ctx.Screen.ShowImage(_ctx.CancellationToken);
            _ctx.Screen.ShowImageImmediate();
            return UniTask.CompletedTask;
        }

        private async UniTask WaitForCutSceneFallback(
            StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
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
