using System;

namespace Novels.Character
{
    internal sealed class CharacterAssetAddressResolver
    {
        private readonly string _contentPrefix;
        private readonly string _episodeId;
        private readonly Content.CharacterAssetProfile _profile;

        internal CharacterAssetAddressResolver(
            string contentPrefix,
            string episodeId,
            Content.CharacterAssetProfile profile)
        {
            _contentPrefix = string.IsNullOrWhiteSpace(contentPrefix)
                ? throw new ArgumentException(
                    "Content prefix must not be empty.",
                    nameof(contentPrefix))
                : contentPrefix;
            _episodeId = string.IsNullOrWhiteSpace(episodeId)
                ? throw new ArgumentException(
                    "Episode ID must not be empty.",
                    nameof(episodeId))
                : episodeId;
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        internal string MainBody(string name, string view, string candidate) =>
            ContentAddressing.ContentAddressConvention.CharacterMainBody(
                _contentPrefix,
                _episodeId,
                name,
                view,
                candidate);

        internal string Emotion(string name, string view, string candidate) =>
            ContentAddressing.ContentAddressConvention.CharacterEmotion(
                _contentPrefix,
                _episodeId,
                name,
                view,
                candidate);

        internal string Clothes(string name, string candidate, int index) =>
            ContentAddressing.ContentAddressConvention.CharacterClothes(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                index);

        internal string Hair(string name, string candidate, string layer) =>
            ContentAddressing.ContentAddressConvention.CharacterHair(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                layer,
                _profile.DefaultHairColor);

        internal string Accessory(string name, string candidate, string layer) =>
            ContentAddressing.ContentAddressConvention.CharacterAccessory(
                _contentPrefix,
                _episodeId,
                name,
                candidate,
                layer);
    }
}
