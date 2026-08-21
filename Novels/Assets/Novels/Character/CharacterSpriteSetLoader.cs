using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterSpriteSetLoader
    {
        private readonly Content.CharacterAssetProfile _profile;
        private readonly ContentAddressing.ContentAddresses _addresses;
        private readonly Func<string, UniTask<Sprite>> _getSprite;
        private readonly Sprite _missingCharacter;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, Sprite> _loadedSprites =
            new(StringComparer.Ordinal);

        internal CharacterSpriteSetLoader(
            Content.CharacterAssetProfile profile,
            ContentAddressing.ContentAddresses addresses,
            Func<string, UniTask<Sprite>> getSprite,
            Sprite missingCharacter,
            CancellationToken cancellationToken)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _addresses = addresses ?? throw new ArgumentNullException(nameof(addresses));
            _getSprite = getSprite ?? throw new ArgumentNullException(nameof(getSprite));
            _missingCharacter = missingCharacter
                ?? throw new ArgumentNullException(nameof(missingCharacter));
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<CharacterSpriteSet> Load(
            string name,
            string view,
            string clothes,
            string hair,
            string accessory,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            var (mainBody, emotion, clothesSprite, hairSprites, accessorySprites) =
                await UniTask.WhenAll(
                    LoadMainBody(name, view, presentation),
                    LoadEmotion(name, view, presentation, appearance),
                    LoadClothes(name, clothes, presentation, appearance),
                    LoadHair(name, hair, presentation, appearance),
                    LoadAccessories(name, accessory, presentation, appearance));
            var sprites = new CharacterSpriteSet(
                mainBody,
                emotion,
                clothesSprite,
                hairSprites,
                accessorySprites);
            if (await RequiresFallback(
                    name,
                    view,
                    clothes,
                    hair,
                    accessory,
                    presentation,
                    appearance,
                    sprites))
                return MissingCharacter();
            return sprites;
        }

        internal async UniTask<Sprite> LoadWardrobeThumbnail(
            StoryContracts.StoryChoiceAction actions,
            string value,
            string mainCharacterView)
        {
            var name = _profile.MainCharacterAssetId;
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
            {
                return await GetSprite(_addresses.CharacterMainBody(
                    name,
                    _profile.ViewPath(value),
                    null)) ?? _missingCharacter;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                return await GetSprite(_addresses.CharacterClothes(name, value, 1))
                    ?? _missingCharacter;
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
            {
                var front = await GetSprite(
                    Hair(name, value, _profile.FrontLayer));
                var back = await GetSprite(
                    Hair(name, value, _profile.BackLayer));
                return front ?? back ?? _missingCharacter;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
            {
                var middle = await GetSprite(
                    _addresses.CharacterAccessory(name, value, _profile.MiddleLayer));
                var front = await GetSprite(
                    _addresses.CharacterAccessory(name, value, _profile.FrontLayer));
                var back = await GetSprite(
                    _addresses.CharacterAccessory(name, value, _profile.BackLayer));
                return middle ?? front ?? back ?? _missingCharacter;
            }
            return await GetSprite(_addresses.CharacterMainBody(name, mainCharacterView, null))
                ?? _missingCharacter;
        }

        private async UniTask<bool> RequiresFallback(
            string name,
            string view,
            string clothes,
            string hair,
            string accessory,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance,
            CharacterSpriteSet sprites)
        {
            if (sprites.MainBody == null)
                return true;
            if (!presentation.IsChild
                && (Missing(appearance.Emotion, sprites.Emotion)
                    || !presentation.RemoveClothes
                    && Missing(appearance.Clothes ?? clothes, sprites.Clothes)
                    || !presentation.RemoveHair
                    && Missing(
                        appearance.Hair ?? hair,
                        sprites.Hair.Back,
                        sprites.Hair.Front)
                    || !presentation.RemoveAccessory
                    && Missing(
                        appearance.Accessories ?? accessory,
                        sprites.Accessories.Back,
                        sprites.Accessories.Middle,
                        sprites.Accessories.Front)))
            {
                return true;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                if (!await ResolvesCandidate(
                        name,
                        view,
                        candidate,
                        presentation.IsChild))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool Missing(string selection, params Sprite[] sprites) =>
            !string.IsNullOrWhiteSpace(selection)
            && Array.TrueForAll(sprites, sprite => sprite == null);

        private async UniTask<bool> ResolvesCandidate(
            string name,
            string view,
            string candidate,
            bool isChild)
        {
            if (isChild)
                view = $"{view}/{_profile.ChildView}";
            var requests = new System.Collections.Generic.List<UniTask<Sprite>>
            {
                GetSprite(_addresses.CharacterMainBody(name, view, candidate)),
                GetSprite(_addresses.CharacterEmotion(name, view, candidate)),
            };
            if (!isChild)
            {
                requests.Add(GetSprite(_addresses.CharacterClothes(name, candidate, 1)));
                requests.Add(GetSprite(Hair(
                    name,
                    candidate,
                    _profile.BackLayer)));
                requests.Add(GetSprite(Hair(
                    name,
                    candidate,
                    _profile.FrontLayer)));
                requests.Add(GetSprite(_addresses.CharacterAccessory(
                    name,
                    candidate,
                    _profile.BackLayer)));
                requests.Add(GetSprite(_addresses.CharacterAccessory(
                    name,
                    candidate,
                    _profile.MiddleLayer)));
                requests.Add(GetSprite(_addresses.CharacterAccessory(
                    name,
                    candidate,
                    _profile.FrontLayer)));
            }
            var sprites = await UniTask.WhenAll(requests);
            return Array.Exists(sprites, sprite => sprite != null);
        }

        private async UniTask<Sprite> LoadMainBody(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            var sprite = await GetSprite(_addresses.CharacterMainBody(name, view, null));
            if (presentation.IsChild)
            {
                view = $"{view}/{_profile.ChildView}";
                sprite = await GetSprite(_addresses.CharacterMainBody(name, view, null)) ?? sprite;
            }
            foreach (var candidate in presentation.AssetCandidates)
            {
                var candidateSprite = await GetSprite(
                    _addresses.CharacterMainBody(name, view, candidate));
                if (candidateSprite == null)
                    continue;
                return candidateSprite;
            }
            return sprite;
        }

        private async UniTask<Sprite> LoadEmotion(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            var inheritedEmotion = presentation.IsChild
                ? null
                : appearance.Emotion;
            if (presentation.IsChild)
                view = $"{view}/{_profile.ChildView}";
            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(_addresses.CharacterEmotion(name, view, candidate));
                if (sprite == null)
                    continue;
                if (!presentation.IsChild)
                    appearance.Emotion = candidate;
                return sprite;
            }

            // Adult appearance state must not leak into the child asset tree.
            // Missing authored adult variants keep the last resolvable emotion.
            return await GetSprite(_addresses.CharacterEmotion(name, view, inheritedEmotion));
        }

        private async UniTask<Sprite> LoadClothes(
            string name,
            string clothes,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance,
            int index = 1)
        {
            if (presentation.IsChild)
            {
                clothes = null;
                appearance.Clothes = null;
            }
            else if (presentation.RemoveClothes)
            {
                appearance.Clothes = null;
            }
            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(_addresses.CharacterClothes(name, candidate, index));
                if (sprite == null)
                    continue;
                appearance.Clothes = candidate;
                return sprite;
            }
            return await GetSprite(
                _addresses.CharacterClothes(name, appearance.Clothes ?? clothes, index));
        }

        private async UniTask<CharacterHairSprites> LoadHair(
            string name,
            string hair,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            if (presentation.IsChild)
            {
                appearance.Hair = null;
                hair = null;
            }
            else if (presentation.RemoveHair)
            {
                appearance.Hair = null;
            }
            foreach (var candidate in presentation.AssetCandidates)
            {
                var (backCandidate, frontCandidate) = await UniTask.WhenAll(
                    GetSprite(Hair(name, candidate, _profile.BackLayer)),
                    GetSprite(Hair(name, candidate, _profile.FrontLayer)));
                if (backCandidate == null && frontCandidate == null)
                    continue;
                appearance.Hair = candidate;
                return new CharacterHairSprites(backCandidate, frontCandidate);
            }
            var resolved = presentation.IsChild
                ? null
                : appearance.Hair
                    ?? (string.IsNullOrWhiteSpace(hair)
                        ? _profile.DefaultHairStyle
                        : hair);
            var (back, front) = await UniTask.WhenAll(
                GetSprite(Hair(name, resolved, _profile.BackLayer)),
                GetSprite(Hair(name, resolved, _profile.FrontLayer)));
            return new CharacterHairSprites(back, front);
        }

        private async UniTask<CharacterAccessorySprites> LoadAccessories(
            string name,
            string accessory,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            if (presentation.IsChild || presentation.RemoveAccessory)
                appearance.Accessories = null;
            foreach (var candidate in presentation.AssetCandidates)
            {
                var (backCandidate, middleCandidate, frontCandidate) =
                    await UniTask.WhenAll(
                        GetSprite(_addresses.CharacterAccessory(
                            name, candidate, _profile.BackLayer)),
                        GetSprite(_addresses.CharacterAccessory(
                            name, candidate, _profile.MiddleLayer)),
                        GetSprite(_addresses.CharacterAccessory(
                            name, candidate, _profile.FrontLayer)));
                if (backCandidate == null
                    && middleCandidate == null
                    && frontCandidate == null)
                    continue;
                appearance.Accessories = candidate;
                return new CharacterAccessorySprites(
                    backCandidate,
                    middleCandidate,
                    frontCandidate);
            }
            var resolved = appearance.Accessories ?? accessory;
            var (back, middle, front) = await UniTask.WhenAll(
                GetSprite(_addresses.CharacterAccessory(
                    name, resolved, _profile.BackLayer)),
                GetSprite(_addresses.CharacterAccessory(
                    name, resolved, _profile.MiddleLayer)),
                GetSprite(_addresses.CharacterAccessory(
                    name, resolved, _profile.FrontLayer)));
            return new CharacterAccessorySprites(back, middle, front);
        }

        private string Hair(string name, string candidate, string layer) =>
            _addresses.CharacterHair(
                name, candidate, layer, _profile.DefaultHairColor);

        private CharacterSpriteSet MissingCharacter() => new(
            _missingCharacter,
            null,
            null,
            new CharacterHairSprites(null, null),
            new CharacterAccessorySprites(null, null, null));

        private async UniTask<Sprite> GetSprite(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            if (_loadedSprites.TryGetValue(path, out var sprite))
                return sprite;
            sprite = await _getSprite(path)
                .AttachExternalCancellation(_cancellationToken);
            _loadedSprites[path] = sprite;
            return sprite;
        }
    }
}
