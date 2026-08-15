namespace Novels
{
    internal partial class Entity
    {
        private static Content.NovelDefinition CreateNovelDefinition(Data data)
        {
            var episode = new Content.EpisodeDefinition(
                data.StoryTextPath,
                data.StoryTextPath,
                "1",
                data.NovelsBubbleBundleName,
                data.NovelsLocationBundleName,
                data.NovelsCharacterBundleName,
                data.NovelsNotificationBundleName);

            return new Content.NovelDefinition(
                data.Prefix,
                data.Prefix,
                data.MainCharacter,
                data.NovelsLoadingBundleName,
                data.NovelsSettingBundleName,
                data.NovelsLocalizationBundleName,
                episode);
        }
    }
}
