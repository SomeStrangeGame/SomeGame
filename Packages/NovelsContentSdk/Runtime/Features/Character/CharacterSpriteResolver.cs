using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterSpriteResolver
    {
        private readonly Content.CharacterAssetProfile _profile;
        private readonly CharacterAppearanceStore _appearances;
        private readonly CharacterSpriteSetLoader _sprites;
        private readonly CharacterThumbnailLoader _thumbnails;

        internal CharacterSpriteResolver(
            string contentPrefix,
            Content.CharacterAssetProfile profile,
            Func<string, UniTask<Sprite>> getSprite,
            Func<UniTask<CharacterSpriteTrimManifest>> getSpriteTrimManifest,
            Sprite missingCharacter,
            CancellationToken cancellationToken)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _appearances = new CharacterAppearanceStore();
            var addresses = new ContentAddressing.ContentAddresses(contentPrefix);
            _sprites = new CharacterSpriteSetLoader(
                profile,
                addresses,
                getSprite,
                getSpriteTrimManifest,
                missingCharacter,
                cancellationToken);
            _thumbnails = new CharacterThumbnailLoader(
                profile,
                addresses,
                getSprite,
                missingCharacter,
                cancellationToken);
        }

        internal void ClearClothes() =>
            _appearances.ClearClothes(_profile.MainCharacterAssetId);

        internal void ClearHair() =>
            _appearances.ClearHair(_profile.MainCharacterAssetId);

        internal void ClearAccessories() =>
            _appearances.ClearAccessories(_profile.MainCharacterAssetId);

        internal void ClearLoadedSprites() => _sprites.ClearLoadedSprites();

        internal UniTask<CharacterSpriteSet> Resolve(
            StoryContracts.CharacterRenderRequest request,
            string mainCharacterView,
            string mainCharacterClothes,
            string mainCharacterHair,
            string mainCharacterAccessory)
        {
            var name = request.Name;
            var view = _profile.ViewRoot;
            var clothes = string.Empty;
            var hair = string.Empty;
            var accessory = string.Empty;
            if (request.Role == StoryContracts.StorySpeakerRole.MainCharacter
                || request.Role == StoryContracts.StorySpeakerRole.Wardrobe)
            {
                name = _profile.MainCharacterAssetId;
                view = mainCharacterView;
                clothes = mainCharacterClothes;
                hair = mainCharacterHair;
                accessory = mainCharacterAccessory;
            }
            name ??= string.Empty;
            name = ContentAddressing.TechnicalAssetIdConvention.Canonicalize(name);
            return _sprites.Load(
                name,
                view,
                clothes,
                hair,
                accessory,
                request.Presentation,
                _appearances.Get(name));
        }

        internal UniTask<Sprite> LoadWardrobeThumbnail(
            StoryContracts.StoryChoiceAction actions,
            string value,
            string mainCharacterView) =>
            _thumbnails.Load(actions, value, mainCharacterView);
    }
}
