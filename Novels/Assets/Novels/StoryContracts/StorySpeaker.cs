namespace Novels.StoryContracts
{
    public static class StorySpeakerRoleResolver
    {
        public static StorySpeakerRole Resolve(
            string speaker,
            DialoguePresentation presentation,
            string mainCharacter)
        {
            if (StorySpeakers.IsWardrobe(speaker)
                || presentation == DialoguePresentation.Wardrobe)
            {
                return StorySpeakerRole.Wardrobe;
            }
            if (StorySpeakers.IsNarrator(speaker)
                || presentation == DialoguePresentation.Narrator)
            {
                return StorySpeakerRole.Narrator;
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

        public static bool ShowsCharacter(StorySpeakerRole role) =>
            role != StorySpeakerRole.Narrator;
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
