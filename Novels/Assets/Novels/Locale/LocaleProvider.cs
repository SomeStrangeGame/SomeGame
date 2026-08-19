using System;
using System.Collections.Generic;
using System.Globalization;

namespace Novels.Locale
{
    public static class LocalePolicy
    {
        public const string FallbackLocale = "en";

        private static readonly IReadOnlyList<string> _supportedLocales =
            Array.AsReadOnly(new[]
            {
                FallbackLocale,
                "ru",
            });

        public static IReadOnlyList<string> SupportedLocales => _supportedLocales;
    }

    public sealed class LocaleProvider
    {
        public LocaleProvider(CultureInfo culture)
        {
            Code = Normalize(culture?.TwoLetterISOLanguageName);
        }

        public string Code { get; }

        public static string Normalize(string locale) =>
            string.IsNullOrWhiteSpace(locale)
                ? LocalePolicy.FallbackLocale
                : locale.Trim().ToLowerInvariant();

        public static string NormalizeRequired(string locale) =>
            string.IsNullOrWhiteSpace(locale)
                ? throw new ArgumentException("Locale must not be empty.", nameof(locale))
                : locale.Trim().ToLowerInvariant();
    }

    public static class LocaleSelector
    {
        public static bool TryFind<T>(
            IEnumerable<T> values,
            Func<T, string> getLocale,
            string locale,
            out T selected)
        {
            if (getLocale == null)
                throw new ArgumentNullException(nameof(getLocale));
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentException("Locale must not be empty.", nameof(locale));
            var requested = LocaleProvider.Normalize(locale);
            var fallbackLocale = LocalePolicy.FallbackLocale;
            var hasRequested = false;
            var requestedValue = default(T);
            var hasFallback = false;
            var fallback = default(T);
            var locales = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values ?? Array.Empty<T>())
            {
                var valueLocale = LocaleProvider.NormalizeRequired(getLocale(value));
                if (!locales.Add(valueLocale))
                {
                    throw new InvalidOperationException(
                        $"Duplicate localization locale '{valueLocale}'.");
                }
                if (string.Equals(
                        valueLocale,
                        fallbackLocale,
                        StringComparison.OrdinalIgnoreCase))
                {
                    fallback = value;
                    hasFallback = true;
                }
                if (string.Equals(
                        valueLocale,
                        requested,
                        StringComparison.OrdinalIgnoreCase))
                {
                    requestedValue = value;
                    hasRequested = true;
                }
            }
            if (hasRequested)
            {
                selected = requestedValue;
                return true;
            }
            selected = fallback;
            return hasFallback;
        }
    }
}
