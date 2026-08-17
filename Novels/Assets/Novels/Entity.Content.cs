namespace Novels
{
    internal partial class Entity
    {
        private static Content.NovelDefinition CreateNovelDefinition(Data data)
        {
            var media = new Content.EpisodeMediaDefinition(
                new[]
                {
                    "alexasroom_water", "arrivalInatlantis", "atlantisdestroy",
                    "bike_water", "bikeinmove_water", "boat", "boatinmove",
                    "boatsunset", "bridge1_water", "immersionatlantis",
                    "islandunderwater_water", "jellyfish", "lightingmeteor",
                    "mainhall_water", "mainroom", "meteor", "oldtown_water",
                    "philshouse", "pier", "prohod_dark_water", "sandybeach",
                    "shark", "shark1", "sharkeat", "tonel_water",
                    "tonelwithpath", "underwater_water", "wardrobe_dark_water",
                    "wardrobe_water", "whirlpoolnight", "window in",
                },
                new[] { ".wav", ".WAV", ".mp3" });
            var episode = new Content.EpisodeDefinition(
                data.EpisodeId,
                data.StoryTextPath,
                data.ContentVersion,
                data.NovelsBubbleBundleName,
                data.NovelsLocationBundleName,
                data.NovelsCharacterBundleName,
                data.NovelsNotificationBundleName,
                media);

            return new Content.NovelDefinition(
                data.NovelId,
                data.Prefix,
                data.MainCharacter,
                data.NovelsLoadingBundleName,
                data.NovelsSettingBundleName,
                data.NovelsLocalizationBundleName,
                episode);
        }
    }
}
