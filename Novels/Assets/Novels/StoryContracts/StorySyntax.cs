namespace Novels.StoryContracts
{
    public static class StorySyntaxTokens
    {
        public const string InlineComment = "//";
    }

    public static class StorySpeakers
    {
        public const string Wardrobe = "Wardrobe";
        public const string Narrator = "...";
        public const string NarratorLegacy = "..";
        public const string EpisodeDescription = "Описание";

        public static bool IsNarrator(string value) =>
            value == Narrator
            || value == NarratorLegacy
            || value == EpisodeDescription;
    }

    public static class StoryDisplayNames
    {
        public static bool IsKnown(string value) => value switch
        {
            "Анпу" => true,
            "Божество" => true,
            "Воин" => true,
            "Другой неизвестный" => true,
            "Женский голос" => true,
            "Женщина" => true,
            "Меджай" => true,
            "Мужской голос" => true,
            "Мужчина" => true,
            "Неизвестный" => true,
            "Незнакомец" => true,
            "Незнакомец в плаще" => true,
            "Пленник" => true,
            "Старуха" => true,
            "Торговец" => true,
            "Хозяйка дома" => true,
            _ => false,
        };
    }

    public static class StoryArguments
    {
        public const string Child = "маленькая";
        public const string Disclaimer = "дисклеймер";
        public const string Hint = "подсказка";
        public const string Thoughts = "мысли";
        public const string WhiteBackground = "white";
        public const string WhiteBackgroundRussian = "белый";
        public const string EndCutScene = "end";
        public const string RemoveClothes = "убрать одежду";
        public const string RemoveHair = "убрать причёску";
        public const string RemoveHairLegacy = "убрать прическу";
        public const string RemoveAccessory = "убрать аксессуар";
        public const string PositionLeft = "слева";
        public const string PositionRight = "справа";
        public const string PositionCenter = "по центру";
        public const string TimedChoicePrefix = "на время";
        public const string HideCharacter = "невидимка";
        public const string ShowCharacter = "убрать невидимку";
        public const string ShowCharacterLegacy = "снять невидимку";
        // TODO: Replace this temporary dialogue control with wardrobe state handling.
        public const string ChangeClothes = "переодеть";
    }

    public static class StoryChoiceActions
    {
        public const string SelectAppearance = "Выбери внешность";
        public const string SelectAppearanceFormal = "Выберите внешность";
        public const string SelectClothes = "Выбери одежду";
        public const string SelectClothesFormal = "Выберите одежду";
        public const string SelectHair = "Выбери причёску";
        public const string SelectHairLegacy = "Выбери прическу";
        public const string SelectHairFormal = "Выберите причёску";
        public const string SelectHairFormalLegacy = "Выберите прическу";
    }

    public static class StoryCameraActions
    {
        public const string FadeIn = "fadein";
        public const string LeftRight = "leftright";
        public const string RightLeft = "rightleft";
        public const string ToCenter = "tocenter";
        public const string ToLeft = "toleft";
        public const string ToRight = "toright";
        public const string Shaking = "shaking";
        public const string Injury = "injury";
        public const string Splashes = "splashes";
        public const string FadeInRussian = "Затемнение";
        public const string LeftRightRussian = "слева направо";
        public const string RightLeftRussian = "справа налево";
        public const string ToCenterRussian = "сместить в центр";
        public const string MoveToRightRussian = "сместить вправо";
        public const string ShakingRussian = "Тряска";
        public const string ShakingScreenRussian = "Тряска экрана";
        public const string InjuryRussian = "Ранение";
        public const string SplashesRussian = "брызги";
        public const string WavesRussian = "волны";
        public const string WhiteFlashRussian = "Белая вспышка";
        public const string FlashRussian = "Вспышка";
    }
}
