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

        internal CharacterSpriteResolver(
            string contentPrefix,
            string episodeId,
            Content.CharacterAssetProfile profile,
            Func<string, UniTask<Sprite>> getSprite,
            Sprite missingCharacter,
            CancellationToken cancellationToken)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _appearances = new CharacterAppearanceStore();
            var addresses = new CharacterAssetAddressResolver(
                contentPrefix,
                episodeId,
                profile);
            _sprites = new CharacterSpriteSetLoader(
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

        internal UniTask<CharacterSpriteSet> Resolve(
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
            name = ContentAddressing.TechnicalAssetIdConvention.Canonicalize(name);
            return _sprites.Load(
                name,
                view,
                clothes,
                hair,
                request.Presentation,
                _appearances.Get(name));
        }
    }
}
