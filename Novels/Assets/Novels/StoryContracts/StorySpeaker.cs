namespace Novels.StoryContracts
{
    public enum StorySpeakerRole
    {
        MainCharacter,
        Character,
        Narrator,
        Wardrobe,
    }

    public enum StoryCharacterPosition
    {
        Left,
        Right,
        Center,
    }

    public sealed class CharacterRenderRequest
    {
        public CharacterRenderRequest(
            string name,
            StorySpeakerRole role,
            StoryCharacterPosition position,
            CharacterPresentation presentation)
        {
            Name = name ?? string.Empty;
            Role = role;
            Position = position;
            Presentation = presentation;
        }

        public string Name { get; }
        public StorySpeakerRole Role { get; }
        public StoryCharacterPosition Position { get; }
        public CharacterPresentation Presentation { get; }
    }
}
