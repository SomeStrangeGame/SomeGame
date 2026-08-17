using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Character.Entity CreateCharacter(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite)
        {
            var character = new Character.Entity(new Character.Entity.Ctx
            {
                ScreenPrefab = screenPrefab,
                ContentPrefix = _definition.Prefix,
                GetSprite = getSprite,
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(owner);
            character.Init();

            return character;
        }
    }
}
