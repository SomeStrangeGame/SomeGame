using System;
using System.Collections.Generic;
using System.Globalization;

namespace Novels
{
    internal enum ApplicationText
    {
        CatalogLoading,
        CatalogLoadFailed,
        Retry,
        ChooseEpisode,
        ContentAvailable,
    }

    internal sealed class ApplicationLocalization
    {
        private readonly IReadOnlyDictionary<ApplicationText, string> _values;

        internal ApplicationLocalization(CultureInfo culture)
        {
            var russian = string.Equals(
                culture?.TwoLetterISOLanguageName,
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
                }
                : new Dictionary<ApplicationText, string>
                {
                    [ApplicationText.CatalogLoading] = "Loading story catalog…",
                    [ApplicationText.CatalogLoadFailed] =
                        "Could not load the catalog. Check your connection.",
                    [ApplicationText.Retry] = "Retry",
                    [ApplicationText.ChooseEpisode] = "Choose an episode",
                    [ApplicationText.ContentAvailable] = "Available",
                };
        }

        internal string Get(ApplicationText key) => _values[key];
    }
}
