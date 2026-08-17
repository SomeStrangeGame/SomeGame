using System;

namespace Novels.Content
{
    public sealed class CharacterAssetProfile
    {
        private const string _defaultMainCharacterAssetId = "MainCharacter";
        private const string _defaultViewRoot = "View";
        private const string _defaultChildView = "Child";
        private const string _defaultBackLayer = "Back";
        private const string _defaultMiddleLayer = "Middle";
        private const string _defaultFrontLayer = "Front";
        private const string _defaultHairColor = "Блонд";

        public CharacterAssetProfile(
            string mainCharacterAssetId = null,
            string viewRoot = null,
            string childView = null,
            string backLayer = null,
            string middleLayer = null,
            string frontLayer = null,
            string defaultHairColor = null)
        {
            MainCharacterAssetId = OrDefault(
                mainCharacterAssetId,
                _defaultMainCharacterAssetId);
            ViewRoot = OrDefault(viewRoot, _defaultViewRoot);
            ChildView = OrDefault(childView, _defaultChildView);
            BackLayer = OrDefault(backLayer, _defaultBackLayer);
            MiddleLayer = OrDefault(middleLayer, _defaultMiddleLayer);
            FrontLayer = OrDefault(frontLayer, _defaultFrontLayer);
            DefaultHairColor = OrDefault(defaultHairColor, _defaultHairColor);
        }

        public string MainCharacterAssetId { get; }
        public string ViewRoot { get; }
        public string ChildView { get; }
        public string BackLayer { get; }
        public string MiddleLayer { get; }
        public string FrontLayer { get; }
        public string DefaultHairColor { get; }

        public string ViewPath(string view) =>
            string.IsNullOrWhiteSpace(view) ? ViewRoot : $"{ViewRoot}/{view}";

        private static string OrDefault(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
