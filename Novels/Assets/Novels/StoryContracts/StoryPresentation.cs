using System;

namespace Novels.StoryContracts
{
    public enum DialoguePresentation
    {
        Character,
        Narrator,
        Wardrobe,
        Disclaimer,
        Hint,
        Thoughts,
    }

    [Flags]
    public enum StoryChoiceAction
    {
        None = 0,
        SelectAppearance = 1 << 0,
        SelectClothes = 1 << 1,
        SelectHair = 1 << 2,
        SelectAccessory = 1 << 3,
    }
}
