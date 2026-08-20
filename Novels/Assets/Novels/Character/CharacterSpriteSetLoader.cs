using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterSpriteSetLoader
    {
        private readonly Content.CharacterAssetProfile _profile;
        private readonly CharacterAssetAddressResolver _addresses;
        private readonly Func<string, UniTask<Sprite>> _getSprite;
        private readonly Sprite _missingCharacter;
        private readonly CancellationToken _cancellationToken;

        internal CharacterSpriteSetLoader(
            Content.CharacterAssetProfile profile,
            CharacterAssetAddressResolver addresses,
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
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            var (mainBody, emotion, clothesSprite, hairSprites, accessorySprites) =
                await UniTask.WhenAll(
                    LoadMainBody(name, view, presentation),
                    LoadEmotion(name, view, presentation, appearance),
                    LoadClothes(name, clothes, presentation, appearance),
                    LoadHair(name, hair, presentation, appearance),
                    LoadAccessories(name, presentation, appearance));
            if (await RequiresFallback(
                    name,
                    view,
                    clothes,
                    hair,
                    presentation,
                    appearance,
                    mainBody,
                    emotion,
                    clothesSprite,
                    hairSprites,
                    accessorySprites))
            {
                return new CharacterSpriteSet(
                    _missingCharacter,
                    null,
                    null,
                    new CharacterHairSprites(null, null),
                    new CharacterAccessorySprites(null, null, null));
            }
            return new CharacterSpriteSet(
                mainBody,
                emotion,
                clothesSprite,
                hairSprites,
                accessorySprites);
        }

        private async UniTask<bool> RequiresFallback(
            string name,
            string view,
            string clothes,
            string hair,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance,
            Sprite mainBody,
            Sprite emotion,
            Sprite clothesSprite,
            CharacterHairSprites hairSprites,
            CharacterAccessorySprites accessorySprites)
        {
            if (mainBody == null)
                return true;
            if (!string.IsNullOrWhiteSpace(appearance.Emotion) && emotion == null)
                return true;
            if (!presentation.IsChild
                && !presentation.RemoveClothes
                && !string.IsNullOrWhiteSpace(appearance.Clothes ?? clothes)
                && clothesSprite == null)
            {
                return true;
            }
            if (!presentation.IsChild
                && !presentation.RemoveHair
                && !string.IsNullOrWhiteSpace(appearance.Hair ?? hair)
                && hairSprites.Back == null
                && hairSprites.Front == null)
            {
                return true;
            }
            if (!presentation.IsChild
                && !presentation.RemoveAccessory
                && !string.IsNullOrWhiteSpace(appearance.Accessories)
                && accessorySprites.Back == null
                && accessorySprites.Middle == null
                && accessorySprites.Front == null)
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
                GetSprite(_addresses.MainBody(name, view, candidate)),
                GetSprite(_addresses.Emotion(name, view, candidate)),
            };
            if (!isChild)
            {
                requests.Add(GetSprite(_addresses.Clothes(name, candidate, 1)));
                requests.Add(GetSprite(_addresses.Hair(
                    name,
                    candidate,
                    _profile.BackLayer)));
                requests.Add(GetSprite(_addresses.Hair(
                    name,
                    candidate,
                    _profile.FrontLayer)));
                requests.Add(GetSprite(_addresses.Accessory(
                    name,
                    candidate,
                    _profile.BackLayer)));
                requests.Add(GetSprite(_addresses.Accessory(
                    name,
                    candidate,
                    _profile.MiddleLayer)));
                requests.Add(GetSprite(_addresses.Accessory(
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
            var sprite = await GetSprite(_addresses.MainBody(name, view, null));
            if (presentation.IsChild)
            {
                view = $"{view}/{_profile.ChildView}";
                sprite = await GetSprite(_addresses.MainBody(name, view, null)) ?? sprite;
            }
            foreach (var candidate in presentation.AssetCandidates)
            {
                var candidateSprite = await GetSprite(
                    _addresses.MainBody(name, view, candidate));
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
            if (presentation.IsChild)
                view = $"{view}/{_profile.ChildView}";
            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(_addresses.Emotion(name, view, candidate));
                if (sprite == null)
                    continue;
                appearance.Emotion = candidate;
                return sprite;
            }

            // Missing authored variants are tolerated while incomplete stories are
            // integrated. Keep the last resolvable emotion for this character.
            return await GetSprite(_addresses.Emotion(name, view, appearance.Emotion));
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
                var sprite = await GetSprite(_addresses.Clothes(name, candidate, index));
                if (sprite == null)
                    continue;
                appearance.Clothes = candidate;
                break;
            }
            return await GetSprite(
                _addresses.Clothes(name, appearance.Clothes ?? clothes, index));
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
                    GetSprite(_addresses.Hair(name, candidate, _profile.BackLayer)),
                    GetSprite(_addresses.Hair(name, candidate, _profile.FrontLayer)));
                if (backCandidate == null && frontCandidate == null)
                    continue;
                appearance.Hair = candidate;
                break;
            }
            var resolved = appearance.Hair ?? hair;
            var (back, front) = await UniTask.WhenAll(
                GetSprite(_addresses.Hair(name, resolved, _profile.BackLayer)),
                GetSprite(_addresses.Hair(name, resolved, _profile.FrontLayer)));
            return new CharacterHairSprites(back, front);
        }

        private async UniTask<CharacterAccessorySprites> LoadAccessories(
            string name,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            if (presentation.IsChild || presentation.RemoveAccessory)
                appearance.Accessories = null;
            foreach (var candidate in presentation.AssetCandidates)
            {
                var (backCandidate, middleCandidate, frontCandidate) =
                    await UniTask.WhenAll(
                        GetSprite(_addresses.Accessory(
                            name, candidate, _profile.BackLayer)),
                        GetSprite(_addresses.Accessory(
                            name, candidate, _profile.MiddleLayer)),
                        GetSprite(_addresses.Accessory(
                            name, candidate, _profile.FrontLayer)));
                if (backCandidate == null
                    && middleCandidate == null
                    && frontCandidate == null)
                    continue;
                appearance.Accessories = candidate;
                break;
            }
            var (back, middle, front) = await UniTask.WhenAll(
                GetSprite(_addresses.Accessory(
                    name, appearance.Accessories, _profile.BackLayer)),
                GetSprite(_addresses.Accessory(
                    name, appearance.Accessories, _profile.MiddleLayer)),
                GetSprite(_addresses.Accessory(
                    name, appearance.Accessories, _profile.FrontLayer)));
            return new CharacterAccessorySprites(back, middle, front);
        }

        private UniTask<Sprite> GetSprite(string path) =>
            string.IsNullOrWhiteSpace(path)
                ? UniTask.FromResult<Sprite>(null)
                : _getSprite(path).AttachExternalCancellation(_cancellationToken);
    }
}
