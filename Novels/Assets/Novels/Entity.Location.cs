using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Location.Entity CreateLocation(IBaseDisposable owner, GameObject screenPrefab, Func<string, UniTask<Sprite>> getSprite, Func<string, UniTask<string>> resolveVideoUrl)
        {
            var location = new Location.Entity(new Location.Entity.Ctx
            {
                ScreenPrefab = screenPrefab,
                TargetCamera = Camera.main,
                GetSprite = getSprite,
                ResolveVideoUrl = resolveVideoUrl,
                CancellationToken = _ctx.CancellationToken,

                OnError = _ctx.OnError,
            }).AddTo(owner);
            location.Init();

            return location;
        }
    }
}
