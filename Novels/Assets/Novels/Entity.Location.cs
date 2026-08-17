using System;
using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Location.Entity CreateLocation(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite,
            Func<string, UniTask<string>> resolveVideoUrl,
            CancellationToken cancellationToken)
        {
            var location = new Location.Entity(new Location.Entity.Ctx
            {
                ScreenPrefab = screenPrefab,
                TargetCamera = _ctx.TargetCamera,
                GetSprite = getSprite,
                ResolveVideoUrl = resolveVideoUrl,
                CancellationToken = cancellationToken,

                OnError = ReportError,
            }).AddTo(owner);
            location.Init();

            return location;
        }
    }
}
