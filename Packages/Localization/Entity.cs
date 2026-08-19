using System;
using System.Collections.Generic;

namespace Localization
{
    public sealed class Entity
    {
        public struct Ctx
        {
            public string Locale;
            public LocalizationData LocalizationSO;
            public bool RequireExactLocale;
        }

        private readonly IReadOnlyDictionary<string, string> _values;

        public Entity(Ctx ctx)
        {
            if (ctx.LocalizationSO == null)
                throw new ArgumentNullException(nameof(ctx.LocalizationSO));
            _values = ctx.LocalizationSO.CreateSnapshot(
                ctx.Locale,
                ctx.RequireExactLocale);
        }

        public string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key ?? string.Empty;
            return _values.TryGetValue(key, out var value) ? value : key;
        }

        public string GetRequiredValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Localization key must not be empty.", nameof(key));
            if (!_values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Required localization key '{key}' is missing or empty.");
            return value;
        }
    }
}
