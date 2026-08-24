using UnityEngine;

namespace Novels.Character
{
    internal sealed class CharacterAppearanceState
    {
        internal string Emotion;
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

        internal bool IsEmpty => Back == null && Front == null;
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

        internal bool IsEmpty => Back == null && Middle == null && Front == null;
    }

    internal readonly struct CharacterSpriteSet
    {
        internal readonly Sprite MainBody;
        internal readonly Sprite Emotion;
        internal readonly Sprite Clothes;
        internal readonly CharacterHairSprites Hair;
        internal readonly CharacterAccessorySprites Accessories;
        internal readonly CharacterSpriteTrimLayouts TrimLayouts;

        internal CharacterSpriteSet(
            Sprite mainBody,
            Sprite emotion,
            Sprite clothes,
            CharacterHairSprites hair,
            CharacterAccessorySprites accessories,
            CharacterSpriteTrimLayouts trimLayouts = default)
        {
            MainBody = mainBody;
            Emotion = emotion;
            Clothes = clothes;
            Hair = hair;
            Accessories = accessories;
            TrimLayouts = trimLayouts;
        }
    }

    internal readonly struct CharacterSpriteTrimLayouts
    {
        internal readonly CharacterSpriteTrimLayout MainBody;
        internal readonly CharacterSpriteTrimLayout Emotion;
        internal readonly CharacterSpriteTrimLayout Clothes;
        internal readonly CharacterSpriteTrimLayout BackHair;
        internal readonly CharacterSpriteTrimLayout FrontHair;
        internal readonly CharacterSpriteTrimLayout BackAccessory;
        internal readonly CharacterSpriteTrimLayout MiddleAccessory;
        internal readonly CharacterSpriteTrimLayout FrontAccessory;

        internal CharacterSpriteTrimLayouts(
            CharacterSpriteTrimLayout mainBody,
            CharacterSpriteTrimLayout emotion,
            CharacterSpriteTrimLayout clothes,
            CharacterSpriteTrimLayout backHair,
            CharacterSpriteTrimLayout frontHair,
            CharacterSpriteTrimLayout backAccessory,
            CharacterSpriteTrimLayout middleAccessory,
            CharacterSpriteTrimLayout frontAccessory)
        {
            MainBody = mainBody;
            Emotion = emotion;
            Clothes = clothes;
            BackHair = backHair;
            FrontHair = frontHair;
            BackAccessory = backAccessory;
            MiddleAccessory = middleAccessory;
            FrontAccessory = frontAccessory;
        }
    }
}
