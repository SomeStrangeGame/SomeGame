using System;
using System.Collections.Generic;

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
        private readonly IReadOnlyDictionary<ApplicationText, string> _values;

        internal ApplicationLocalization(string locale)
        {
            var russian = string.Equals(
                Locale.LocaleProvider.Normalize(locale),
                "ru",
                StringComparison.OrdinalIgnoreCase);
            _values = russian
                ? new Dictionary<ApplicationText, string>
                {
                    [ApplicationText.CatalogLoading] = "Загрузка каталога историй…",
                    [ApplicationText.CatalogLoadFailed] =
                        "Не удалось загрузить каталог. Проверьте подключение.",
                    [ApplicationText.Retry] = "Повторить",
                    [ApplicationText.ChooseEpisode] = "Выберите эпизод",
                    [ApplicationText.ContentAvailable] = "Доступно",
                    [ApplicationText.PreparingContent] = "Подготовка истории",
                }
                : new Dictionary<ApplicationText, string>
                {
                    [ApplicationText.CatalogLoading] = "Loading story catalog…",
                    [ApplicationText.CatalogLoadFailed] =
                        "Could not load the catalog. Check your connection.",
                    [ApplicationText.Retry] = "Retry",
                    [ApplicationText.ChooseEpisode] = "Choose an episode",
                    [ApplicationText.ContentAvailable] = "Available",
                    [ApplicationText.PreparingContent] = "Preparing story",
                };
        }

        internal string Get(ApplicationText key) => _values[key];
    }
}
