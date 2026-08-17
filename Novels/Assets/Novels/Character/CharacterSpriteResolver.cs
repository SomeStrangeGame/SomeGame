using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterSpriteResolver
    {
        private readonly string _contentPrefix;
        private readonly string _episodeId;
        private readonly Content.CharacterAssetProfile _profile;
        private readonly Func<string, UniTask<Sprite>> _getSprite;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, CharacterAppearanceState> _appearanceByCharacter =
            new(StringComparer.Ordinal);

        internal CharacterSpriteResolver(
            string contentPrefix,
            string episodeId,
            Content.CharacterAssetProfile profile,
            Func<string, UniTask<Sprite>> getSprite,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(contentPrefix))
                throw new ArgumentException("Content prefix must not be empty.", nameof(contentPrefix));
            _contentPrefix = contentPrefix;
            if (string.IsNullOrWhiteSpace(episodeId))
                throw new ArgumentException("Episode ID must not be empty.", nameof(episodeId));
            _episodeId = episodeId;
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _getSprite = getSprite ?? throw new ArgumentNullException(nameof(getSprite));
            _cancellationToken = cancellationToken;
        }

        internal void ClearClothes() =>
            GetAppearance(_profile.MainCharacterAssetId).Clothes = null;

        internal void ClearHair() =>
            GetAppearance(_profile.MainCharacterAssetId).Hair = null;

        internal async UniTask<CharacterSpriteSet> Resolve(
            StoryContracts.CharacterRenderRequest request,
            string mainCharacterView,
            string mainCharacterClothes,
            string mainCharacterHair)
        {
            var name = request.Name;
            var view = _profile.ViewRoot;
            var clothes = string.Empty;
            var hair = string.Empty;
            if (request.Role == StoryContracts.StorySpeakerRole.MainCharacter
                || request.Role == StoryContracts.StorySpeakerRole.Wardrobe)
            {
                name = _profile.MainCharacterAssetId;
                view = mainCharacterView;
                clothes = mainCharacterClothes;
                hair = mainCharacterHair;
            }

            name ??= string.Empty;
            var appearance = GetAppearance(name);
            var presentation = request.Presentation;
            var (mainBody, emotion, clothesSprite, hairSprites, accessorySprites) =
                await UniTask.WhenAll(
                    ResolveMainBody(name, view, presentation),
                    ResolveEmotion(name, view, presentation),
                    ResolveClothes(name, clothes, presentation, appearance),
                    ResolveHair(name, hair, presentation, appearance),
                    ResolveAccessories(name, presentation, appearance));
            return new CharacterSpriteSet(
                mainBody,
                emotion,
                clothesSprite,
                hairSprites,
                accessorySprites);
        }

        private CharacterAppearanceState GetAppearance(string identity)
        {
            if (_appearanceByCharacter.TryGetValue(identity, out var appearance))
                return appearance;
            appearance = new CharacterAppearanceState();
            _appearanceByCharacter.Add(identity, appearance);
            return appearance;
        }

        private async UniTask<Sprite> ResolveMainBody(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            var sprite = await GetSprite(MainBodyPath(name, view, null));
            if (presentation.IsChild)
            {
                view = $"{view}/{_profile.ChildView}";
                sprite = await GetSprite(MainBodyPath(name, view, null)) ?? sprite;
            }
            foreach (var candidate in presentation.AssetCandidates)
            {
                var candidateSprite = await GetSprite(MainBodyPath(name, view, candidate));
                if (candidateSprite == null)
                    continue;
                sprite = candidateSprite;
                break;
            }
            return sprite;
        }

        private async UniTask<Sprite> ResolveEmotion(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            if (presentation.IsChild)
                view = $"{view}/{_profile.ChildView}";
            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(EmotionPath(name, view, candidate));
                if (sprite != null)
                    return sprite;
            }
            return null;
        }

        private async UniTask<Sprite> ResolveClothes(
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
                var sprite = await GetSprite(ClothesPath(name, candidate, index));
                if (sprite == null)
                    continue;
                appearance.Clothes = candidate;
                break;
            }
            return await GetSprite(ClothesPath(name, appearance.Clothes ?? clothes, index));
        }

        private async UniTask<CharacterHairSprites> ResolveHair(
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
                    GetSprite(HairPath(name, candidate, _profile.BackLayer)),
                    GetSprite(HairPath(name, candidate, _profile.FrontLayer)));
                if (backCandidate == null && frontCandidate == null)
                    continue;
                appearance.Hair = candidate;
                break;
            }
            var resolved = appearance.Hair ?? hair;
            var (back, front) = await UniTask.WhenAll(
                GetSprite(HairPath(name, resolved, _profile.BackLayer)),
                GetSprite(HairPath(name, resolved, _profile.FrontLayer)));
            return new CharacterHairSprites(back, front);
        }

        private async UniTask<CharacterAccessorySprites> ResolveAccessories(
            string name,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            if (presentation.IsChild || presentation.RemoveAccessory)
                appearance.Accessories = null;
            foreach (var candidate in presentation.AssetCandidates)
            {
                var (backCandidate, middleCandidate, frontCandidate) = await UniTask.WhenAll(
                    GetSprite(AccessoriesPath(name, candidate, _profile.BackLayer)),
                    GetSprite(AccessoriesPath(name, candidate, _profile.MiddleLayer)),
                    GetSprite(AccessoriesPath(name, candidate, _profile.FrontLayer)));
                if (backCandidate == null && middleCandidate == null && frontCandidate == null)
                    continue;
                appearance.Accessories = candidate;
                break;
            }
            var (back, middle, front) = await UniTask.WhenAll(
                GetSprite(AccessoriesPath(name, appearance.Accessories, _profile.BackLayer)),
                GetSprite(AccessoriesPath(name, appearance.Accessories, _profile.MiddleLayer)),
                GetSprite(AccessoriesPath(name, appearance.Accessories, _profile.FrontLayer)));
            return new CharacterAccessorySprites(back, middle, front);
        }

        private UniTask<Sprite> GetSprite(string path) =>
            string.IsNullOrWhiteSpace(path)
                ? UniTask.FromResult<Sprite>(null)
                : _getSprite(path).AttachExternalCancellation(_cancellationToken);

        private string MainBodyPath(string name, string view, string candidate) =>
            ContentAddressing.ContentAddressConvention.CharacterMainBody(
                _contentPrefix,
                _episodeId,
                name,
                view,
                candidate);

        private string EmotionPath(string name, string view, string candidate) =>
            ContentAddressing.ContentAddressConvention.CharacterEmotion(
                _contentPrefix,
                _episodeId,
                name,
                view,
                candidate);

        private string ClothesPath(string name, string candidate, int index) =>
            ContentAddressing.ContentAddressConvention.CharacterClothes(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                index);

        private string HairPath(string name, string candidate, string layer) =>
            ContentAddressing.ContentAddressConvention.CharacterHair(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                layer,
                _profile.DefaultHairColor);

        private string AccessoriesPath(string name, string candidate, string layer) =>
            ContentAddressing.ContentAddressConvention.CharacterAccessory(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                layer);
    }
}
