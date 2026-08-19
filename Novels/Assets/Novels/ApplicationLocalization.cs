using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels
{
    internal enum ApplicationText
    {
        CatalogLoading,
        CatalogLoadFailed,
        Retry,
        ChooseEpisode,
        ContentAvailable,
        PreparingContent,
    }

    internal sealed class ApplicationLocalization
    {
        private readonly Localization.Entity _localization;

        internal ApplicationLocalization(string locale)
        {
            var data = Resources.Load<Localization.LocalizationData>(
                ApplicationLocalizationContract.ResourcePath)
                ?? throw new InvalidOperationException(
                    $"Application localization is missing: "
                    + ApplicationLocalizationContract.AssetPath);
            _localization = new Localization.Entity(new Localization.Entity.Ctx
            {
                Locale = locale,
                LocalizationSO = data,
            });
        }

        internal string Get(ApplicationText key) =>
            _localization.GetRequiredValue(Key(key));

        private static string Key(ApplicationText key) => key switch
        {
            ApplicationText.CatalogLoading => ApplicationLocalizationContract.CatalogLoading,
            ApplicationText.CatalogLoadFailed => ApplicationLocalizationContract.CatalogLoadFailed,
            ApplicationText.Retry => ApplicationLocalizationContract.Retry,
            ApplicationText.ChooseEpisode => ApplicationLocalizationContract.ChooseEpisode,
            ApplicationText.ContentAvailable => ApplicationLocalizationContract.ContentAvailable,
            ApplicationText.PreparingContent => ApplicationLocalizationContract.PreparingContent,
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
        };
    }

    public static class ApplicationLocalizationContract
    {
        public const string ResourcePath = "Novels/ApplicationLocalizationData";
        public const string AssetPath =
            "Assets/Resources/Novels/ApplicationLocalizationData.asset";
        public const string CatalogLoading = "application.catalog_loading";
        public const string CatalogLoadFailed = "application.catalog_load_failed";
        public const string Retry = "application.retry";
        public const string ChooseEpisode = "application.choose_episode";
        public const string ContentAvailable = "application.content_available";
        public const string PreparingContent = "application.preparing_content";

        private static readonly IReadOnlyList<string> _requiredKeys =
            Array.AsReadOnly(new[]
            {
                CatalogLoading,
                CatalogLoadFailed,
                Retry,
                ChooseEpisode,
                ContentAvailable,
                PreparingContent,
            });

        public static IReadOnlyList<string> RequiredKeys => _requiredKeys;
    }
}
