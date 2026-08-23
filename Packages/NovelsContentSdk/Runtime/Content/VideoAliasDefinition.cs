using System;

namespace Novels.Content
{
    public sealed class VideoAliasDefinition
    {
        public VideoAliasDefinition(string alias, string target)
        {
            Alias = Require(alias, nameof(alias));
            Target = Require(target, nameof(target));
            if (string.Equals(Alias, Target, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Video alias '{Alias}' points to itself.");
        }

        public string Alias { get; }
        public string Target { get; }

        private static string Require(string value, string parameterName)
        {
            var result = ContentAddressing.TechnicalAssetIdConvention.Canonicalize(value);
            if (string.IsNullOrEmpty(result))
                throw new ArgumentException("Video alias value must not be empty.", parameterName);
            return result;
        }
    }
}
