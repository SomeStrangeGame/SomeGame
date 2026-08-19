namespace Novels.StoryContracts
{
    public static class StorySpeakerRoleResolver
    {
        public static StorySpeakerRole Resolve(
            string speaker,
            DialoguePresentation presentation,
            string mainCharacter)
        {
            if (StorySpeakers.IsNarrator(speaker)
                || presentation == DialoguePresentation.Narrator)
            {
                return StorySpeakerRole.Narrator;
            }
            if (string.Equals(
                    speaker,
                    StorySpeakers.Wardrobe,
                    System.StringComparison.Ordinal)
                || presentation == DialoguePresentation.Wardrobe)
            {
                return StorySpeakerRole.Wardrobe;
            }
            return string.Equals(
                    speaker,
                    mainCharacter,
                    System.StringComparison.Ordinal)
                ? StorySpeakerRole.MainCharacter
                : StorySpeakerRole.Character;
        }

        public static bool RequiresCharacterAsset(StorySpeakerRole role) =>
            role == StorySpeakerRole.Character;
    }

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
