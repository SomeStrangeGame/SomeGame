using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private Character.Entity CreateCharacter(GameObject screenPrefab, Func<string, UniTask<Sprite>> getSprite, Func<string, string, string, string> GetMainBodyPath, Func<string, string, string, string> GetEmotionPath, Func<string, string, int, string> GetClothesPath, Func<string, string, string, string, string> GetHairPath, Func<string, string, string, string> GetAccessoriesPath)
        {
            var character = new Character.Entity(new Character.Entity.Ctx
            {
                ScreenPrefab = screenPrefab,
                GetSprite = getSprite,
                GetMainBodyPath = GetMainBodyPath,
                GetEmotionPath = GetEmotionPath,
                GetClothesPath = GetClothesPath,
                GetHairPath = GetHairPath,
                GetAccessoriesPath = GetAccessoriesPath
            }).AddTo(this);
            character.Init();

            return character;
        }
    }
}
