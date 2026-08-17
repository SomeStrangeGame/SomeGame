namespace Novels.Content
{
    public sealed class CharacterAssetProfile
    {
        private CharacterAssetProfile()
        {
        }

        public static CharacterAssetProfile Default { get; } = new();

        public string MainCharacterAssetId => "MainCharacter";
        public string ViewRoot => "View";
        public string ChildView => "Child";
        public string BackLayer => "Back";
        public string MiddleLayer => "Middle";
        public string FrontLayer => "Front";
        public string DefaultHairColor => "Блонд";

        public string ViewPath(string view) =>
            string.IsNullOrWhiteSpace(view) ? ViewRoot : $"{ViewRoot}/{view}";
    }
}
