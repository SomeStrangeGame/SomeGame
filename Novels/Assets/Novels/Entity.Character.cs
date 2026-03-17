using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Character.Entity> CreateCharacter(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var character = new Character.Entity(new Character.Entity.Ctx
            {
                MainCharacterName = _ctx.Data.MainCharacter,
                GetScreenPrefab = () => bundles.GetBundledPrefab(_ctx.Data.NovelsCharacterBundleName, pathGetter.GetCharacterPrefabAssetName("Screen")),
                GetSprite = assetName => bundles.GetBundledSprite(_ctx.Data.NovelsCharacterBundleName, assetName),
                GetMainBodyPath = pathGetter.GetCharacterMainBodyPath,
                GetEmotionPath = pathGetter.GetCharacterEmotionPath,
                GetClothesPath = pathGetter.GetCharacterClothesPath
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await character.Init();

            return character;
        }
    }
}

