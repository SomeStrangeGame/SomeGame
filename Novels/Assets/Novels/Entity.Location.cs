using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Location.Entity> CreateLocation(GameObject screenPrefab, Func<string, UniTask<Sprite>> getSprite, Func<string, string> getVideoURL)
        {
            var location = new Location.Entity(new Location.Entity.Ctx
            {
                ScreenPrefab = screenPrefab,
                GetSprite = getSprite,
                GetVideoURL = getVideoURL,
                CancellationToken = _ctx.CancellationToken,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
            location.Init();

            return location;
        }
    }
}
