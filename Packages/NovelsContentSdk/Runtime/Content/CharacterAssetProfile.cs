using System;
using System.Collections.Generic;

namespace Novels.Content
{
    public sealed class CharacterAssetProfile
    {
        private static readonly CharacterDefaultAppearanceDefinition _empty =
            new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        private readonly Dictionary<string, CharacterDefaultAppearanceDefinition> _defaults =
            new(StringComparer.Ordinal);

        internal CharacterAssetProfile(
            string mainCharacter,
            IEnumerable<CharacterDefaultAppearanceDefinition> defaults)
        {
            var mainCharacterKey = Canonicalize(mainCharacter);
            foreach (var value in defaults ?? Array.Empty<CharacterDefaultAppearanceDefinition>())
            {
                var key = Canonicalize(value.Character);
                if (key == mainCharacterKey)
                    key = MainCharacterAssetId;
                _defaults[key] = value;
            }
        }

        public static CharacterAssetProfile Default { get; } = new(null, null);

        public string MainCharacterAssetId => "maincharacter";
        public string ViewRoot => "view";
        public string ChildView => "child";
        public string BackLayer => "back";
        public string MiddleLayer => "middle";
        public string FrontLayer => "front";
        public string ViewPath(string view) =>
            string.IsNullOrWhiteSpace(view) ? ViewRoot : $"{ViewRoot}/{view}";

        public CharacterDefaultAppearanceDefinition Defaults(string character)
        {
            var key = Canonicalize(character);
            return _defaults.TryGetValue(key, out var value) ? value : _empty;
        }

        private static string Canonicalize(string value) =>
            ContentAddressing.TechnicalAssetIdConvention.Canonicalize(value ?? string.Empty);
    }
}
