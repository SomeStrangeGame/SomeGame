using System;
using System.Collections.Generic;

namespace Novels.Character
{
    internal sealed class CharacterAppearanceStore
    {
        private readonly Dictionary<string, CharacterAppearanceState> _states =
            new(StringComparer.Ordinal);

        internal CharacterAppearanceState Get(string identity)
        {
            identity ??= string.Empty;
            if (_states.TryGetValue(identity, out var appearance))
                return appearance;
            appearance = new CharacterAppearanceState();
            _states.Add(identity, appearance);
            return appearance;
        }

        internal void ClearClothes(string identity) => Get(identity).Clothes = null;

        internal void ClearHair(string identity) => Get(identity).Hair = null;

        internal void ClearAccessories(string identity) => Get(identity).Accessories = null;
    }
}
