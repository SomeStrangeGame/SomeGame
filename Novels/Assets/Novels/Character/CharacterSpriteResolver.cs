using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterSpriteResolver
    {
        private const string _mainCharacter = "MainCharacter";
        private const string _childView = "Child";
        private const string _backLayer = "Back";
        private const string _middleLayer = "Middle";
        private const string _frontLayer = "Front";
        private const string _defaultHairColor = "Блонд";

        private readonly string _root;
        private readonly Func<string, UniTask<Sprite>> _getSprite;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, CharacterAppearanceState> _appearanceByCharacter =
            new(StringComparer.Ordinal);

        internal CharacterSpriteResolver(
            string contentPrefix,
            Func<string, UniTask<Sprite>> getSprite,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(contentPrefix))
                throw new ArgumentException("Content prefix must not be empty.", nameof(contentPrefix));
            _root = $"Assets/RemoteAssets/Character/{contentPrefix}/Characters";
            _getSprite = getSprite ?? throw new ArgumentNullException(nameof(getSprite));
            _cancellationToken = cancellationToken;
        }

        internal void ClearClothes() => GetAppearance(_mainCharacter).Clothes = null;

        internal void ClearHair() => GetAppearance(_mainCharacter).Hair = null;

        internal async UniTask<CharacterSpriteSet> Resolve(
            StoryContracts.CharacterRenderRequest request,
            string mainCharacterView,
            string mainCharacterClothes,
            string mainCharacterHair)
        {
            var name = request.Name;
            var view = "View";
            var clothes = string.Empty;
            var hair = string.Empty;
            if (request.Role == StoryContracts.StorySpeakerRole.MainCharacter
                || request.Role == StoryContracts.StorySpeakerRole.Wardrobe)
            {
                name = _mainCharacter;
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
                view = $"{view}/{_childView}";
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
                view = $"{view}/{_childView}";
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
                    GetSprite(HairPath(name, candidate, _backLayer)),
                    GetSprite(HairPath(name, candidate, _frontLayer)));
                if (backCandidate == null && frontCandidate == null)
                    continue;
                appearance.Hair = candidate;
                break;
            }
            var resolved = appearance.Hair ?? hair;
            var (back, front) = await UniTask.WhenAll(
                GetSprite(HairPath(name, resolved, _backLayer)),
                GetSprite(HairPath(name, resolved, _frontLayer)));
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
                    GetSprite(AccessoriesPath(name, candidate, _backLayer)),
                    GetSprite(AccessoriesPath(name, candidate, _middleLayer)),
                    GetSprite(AccessoriesPath(name, candidate, _frontLayer)));
                if (backCandidate == null && middleCandidate == null && frontCandidate == null)
                    continue;
                appearance.Accessories = candidate;
                break;
            }
            var (back, middle, front) = await UniTask.WhenAll(
                GetSprite(AccessoriesPath(name, appearance.Accessories, _backLayer)),
                GetSprite(AccessoriesPath(name, appearance.Accessories, _middleLayer)),
                GetSprite(AccessoriesPath(name, appearance.Accessories, _frontLayer)));
            return new CharacterAccessorySprites(back, middle, front);
        }

        private UniTask<Sprite> GetSprite(string path) =>
            string.IsNullOrWhiteSpace(path)
                ? UniTask.FromResult<Sprite>(null)
                : _getSprite(path).AttachExternalCancellation(_cancellationToken);

        private string MainBodyPath(string name, string view, string candidate) =>
            string.IsNullOrEmpty(name)
                ? string.Empty
                : $"{_root}/{name}/{view}/{candidate ?? "Main"}.png";

        private string EmotionPath(string name, string view, string candidate) =>
            BuildNamedPath(name, candidate, value => $"{_root}/{name}/{view}/Emotions/{value}.png");

        private string ClothesPath(string name, string candidate, int index) =>
            BuildNamedPath(name, candidate, value => $"{_root}/{name}/Clothes/{value}/{index}.png");

        private string HairPath(string name, string candidate, string layer) =>
            BuildNamedPath(
                name,
                candidate,
                value => $"{_root}/{name}/Hairs/{layer}/{value}/{_defaultHairColor}.png");

        private string AccessoriesPath(string name, string candidate, string layer) =>
            BuildNamedPath(
                name,
                candidate,
                value => $"{_root}/{name}/Accessories/{layer}/{value}.png");

        private static string BuildNamedPath(
            string name,
            string candidate,
            Func<string, string> build)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(candidate))
                return string.Empty;
            return build(char.ToUpperInvariant(candidate[0]) + candidate.Substring(1).ToLowerInvariant());
        }
    }
}
