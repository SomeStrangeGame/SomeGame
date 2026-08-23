using System;
using System.Text;

namespace Bundles
{
    internal static class ContentCacheNamespace
    {
        internal static string CreatePrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var source = value.Trim();
            var result = new StringBuilder(source.Length + 1);
            foreach (var character in source)
            {
                if (char.IsLetterOrDigit(character) || character is '-' or '_')
                    result.Append(char.ToLowerInvariant(character));
                else
                    result.Append('_');
            }
            if (result.Length == 0)
                throw new ArgumentException("Cache namespace is invalid.", nameof(value));
            result.Append('/');
            return result.ToString();
        }
    }
}
