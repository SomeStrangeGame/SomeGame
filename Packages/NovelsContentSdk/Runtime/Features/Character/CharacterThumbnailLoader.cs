using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterThumbnailLoader
    {
        private readonly Content.CharacterAssetProfile _profile;
        private readonly ContentAddressing.ContentAddresses _addresses;
        private readonly Func<string, UniTask<Sprite>> _getSprite;
        private readonly Sprite _fallback;
        private readonly CancellationToken _cancellationToken;

        internal CharacterThumbnailLoader(
            Content.CharacterAssetProfile profile,
            ContentAddressing.ContentAddresses addresses,
            Func<string, UniTask<Sprite>> getSprite,
            Sprite fallback,
            CancellationToken cancellationToken)
        {
            _profile = profile;
            _addresses = addresses;
            _getSprite = getSprite;
            _fallback = fallback;
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<Sprite> Load(
            StoryContracts.StoryChoiceAction actions,
            string value,
            string mainCharacterView,
            string character)
        {
            var name = string.IsNullOrWhiteSpace(character)
                ? _profile.MainCharacterAssetId
                : ContentAddressing.TechnicalAssetIdConvention.Canonicalize(character);
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
            {
                return await Get(_addresses.CharacterMainBody(
                    name,
                    name == _profile.MainCharacterAssetId
                        ? _profile.ViewPath(value)
                        : _profile.ViewRoot,
                    null)) ?? _fallback;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                return await Get(_addresses.CharacterClothes(name, value, 1)) ?? _fallback;
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
            {
                return await Get(_addresses.CharacterHair(
                           name, value, _profile.FrontLayer,
                           _profile.Defaults(name).HairColor))
                    ?? await Get(_addresses.CharacterHair(
                        name, value, _profile.BackLayer,
                        _profile.Defaults(name).HairColor))
                    ?? _fallback;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
            {
                return await Get(_addresses.CharacterAccessory(
                           name, value, _profile.MiddleLayer))
                    ?? await Get(_addresses.CharacterAccessory(
                        name, value, _profile.FrontLayer))
                    ?? await Get(_addresses.CharacterAccessory(
                        name, value, _profile.BackLayer))
                    ?? _fallback;
            }
            return await Get(_addresses.CharacterMainBody(
                    name,
                    name == _profile.MainCharacterAssetId
                        ? mainCharacterView
                        : _profile.ViewRoot,
                    null))
                ?? _fallback;
        }

        private async UniTask<Sprite> Get(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;
            return await _getSprite(address).AttachExternalCancellation(_cancellationToken);
        }
    }
}
