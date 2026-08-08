using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Character.Entity> CreateCharacter(GameObject screenPrefab, Func<string, UniTask<Sprite>> getSprite, PathGetter.Entity pathGetter)
        {
            var character = new Character.Entity(new Character.Entity.Ctx
            {
                MainCharacterName = _ctx.Data.MainCharacter,
                ScreenPrefab = screenPrefab,
                GetSprite = getSprite,
                GetMainBodyPath = pathGetter.GetCharacterMainBodyPath,
                GetEmotionPath = pathGetter.GetCharacterEmotionPath,
                GetClothesPath = pathGetter.GetCharacterClothesPath,
                GetHairPath = pathGetter.GetCharacterHairPath,
                GetAccessoriesPath = pathGetter.GetCharacterAccessoiresPath
            }).AddTo(this);
            character.Init();

            return character;
        }
    }
}

