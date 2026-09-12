using System;
using System.Text.RegularExpressions;

namespace Novels.WebPlayer
{
    [Serializable]
    public sealed class WebPlayerLaunchConfiguration
    {
        public string storyId;
        public string storyVersion;
        public string manifestUrl;
        public string locale;
        public string profile;
        public string returnUrl;

        public bool TryValidate(out string errorCode)
        {
            if (string.IsNullOrWhiteSpace(storyId)
                || !Regex.IsMatch(storyId, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            {
                errorCode = "invalid_story_id";
                return false;
            }

            if (string.IsNullOrWhiteSpace(storyVersion)
                || !Regex.IsMatch(storyVersion, "^[a-zA-Z0-9._-]+$"))
            {
                errorCode = "invalid_story_version";
                return false;
            }

            var expectedPrefix = $"/content/stories/{storyId}/{storyVersion}/webgl/";
            if (string.IsNullOrWhiteSpace(manifestUrl)
                || !manifestUrl.StartsWith(expectedPrefix, StringComparison.Ordinal)
                || manifestUrl.Contains("://", StringComparison.Ordinal)
                || manifestUrl.StartsWith("//", StringComparison.Ordinal))
            {
                errorCode = "invalid_manifest_url";
                return false;
            }

            var file = manifestUrl.Substring(expectedPrefix.Length);
            if (!Regex.IsMatch(file, "^[a-zA-Z0-9_-][a-zA-Z0-9._-]*\\.json$")
                || file.Contains("..") || storyVersion == "." || storyVersion == "..")
            {
                errorCode = "invalid_manifest_url";
                return false;
            }

            if (!string.Equals(profile, "media-free-v1", StringComparison.Ordinal))
            {
                errorCode = "unsupported_profile";
                return false;
            }

            // Navigation stays with the website. Only simple same-origin routes
            // are accepted, never protocol-relative or encoded external targets.
            if (!string.IsNullOrEmpty(returnUrl)
                && (!Regex.IsMatch(returnUrl, "^/[a-zA-Z0-9/_-]*$") || returnUrl.StartsWith("//")))
            {
                errorCode = "invalid_return_url";
                return false;
            }

            errorCode = string.Empty;
            return true;
        }
    }
}
