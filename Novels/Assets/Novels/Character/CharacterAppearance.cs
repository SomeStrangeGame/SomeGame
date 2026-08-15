using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterAppearanceState
    {
        internal string Clothes;
        internal string Hair;
        internal string Accessories;
    }

    internal readonly struct CharacterHairSprites
    {
        internal readonly Sprite Back;
        internal readonly Sprite Front;

        internal CharacterHairSprites(Sprite back, Sprite front)
        {
            Back = back;
            Front = front;
        }
    }

    internal readonly struct CharacterAccessorySprites
    {
        internal readonly Sprite Back;
        internal readonly Sprite Middle;
        internal readonly Sprite Front;

        internal CharacterAccessorySprites(Sprite back, Sprite middle, Sprite front)
        {
            Back = back;
            Middle = middle;
            Front = front;
        }
    }

    internal readonly struct CharacterSpriteSet
    {
        internal readonly Sprite MainBody;
        internal readonly Sprite Emotion;
        internal readonly Sprite Clothes;
        internal readonly CharacterHairSprites Hair;
        internal readonly CharacterAccessorySprites Accessories;

        internal CharacterSpriteSet(
            Sprite mainBody,
            Sprite emotion,
            Sprite clothes,
            CharacterHairSprites hair,
            CharacterAccessorySprites accessories)
        {
            MainBody = mainBody;
            Emotion = emotion;
            Clothes = clothes;
            Hair = hair;
            Accessories = accessories;
        }
    }
}
