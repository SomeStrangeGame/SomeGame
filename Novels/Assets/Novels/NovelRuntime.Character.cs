using System;
using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private Character.CharacterController CreateCharacter(
            IBaseDisposable owner,
            GameObject screenPrefab,
            Func<string, UniTask<Sprite>> getSprite,
            Sprite missingCharacter,
            CancellationToken cancellationToken)
        {
            var character = new Character.CharacterController(new Character.CharacterController.Dependencies
            {
                ScreenPrefab = screenPrefab,
                ContentPrefix = _definition.Prefix,
                EpisodeId = _episode.Id,
                AssetProfile = _definition.CharacterAssets,
                GetSprite = getSprite,
                MissingCharacter = missingCharacter,
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            character.Init();

            return character;
        }
    }
}
