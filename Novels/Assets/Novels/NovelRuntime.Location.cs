using System;
using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private Location.LocationController CreateLocation(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite,
            Func<string, UniTask<string>> resolveVideoUrl,
            Sprite missingBackground,
            CancellationToken cancellationToken)
        {
            var location = new Location.LocationController(new Location.LocationController.Dependencies
            {
                ScreenPrefab = screenPrefab,
                TargetCamera = _ctx.TargetCamera,
                GetSprite = getSprite,
                ResolveVideoUrl = resolveVideoUrl,
                MissingBackground = missingBackground,
                CancellationToken = cancellationToken,
                CutSceneFallbackDelayMilliseconds =
                    _ctx.RuntimeTuning.CutSceneFallbackDelayMilliseconds,

                OnError = ReportError,
            }).AddTo(owner);
            location.Init();

            return location;
        }
    }
}
