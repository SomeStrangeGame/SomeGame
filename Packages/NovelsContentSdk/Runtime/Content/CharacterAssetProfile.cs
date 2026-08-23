namespace Novels.Content
{
    public sealed class CharacterAssetProfile
    {
        private CharacterAssetProfile()
        {
        }

        public static CharacterAssetProfile Default { get; } = new();

        public string MainCharacterAssetId => "maincharacter";
        public string ViewRoot => "view";
        public string ChildView => "child";
        public string BackLayer => "back";
        public string MiddleLayer => "middle";
        public string FrontLayer => "front";
        public string DefaultHairColor => "блонд";
        public string DefaultHairStyle => "распущенные";

        public string ViewPath(string view) =>
            string.IsNullOrWhiteSpace(view) ? ViewRoot : $"{ViewRoot}/{view}";
    }
}
