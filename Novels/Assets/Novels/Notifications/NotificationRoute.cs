using System;

namespace Novels.Notifications
{
    [Serializable]
    internal sealed class NotificationRoute
    {
        public string notificationId;
        public string storyId;
        public string episodeId;

        internal bool IsValid => !string.IsNullOrWhiteSpace(storyId);

        internal static string Serialize(
            string notificationId,
            string storyId,
            string episodeId = null)
        {
            var route = $"novels://catalog/story/{Uri.EscapeDataString(storyId ?? string.Empty)}";
            var query = string.IsNullOrWhiteSpace(episodeId)
                ? string.Empty
                : $"episode={Uri.EscapeDataString(episodeId)}";
            if (!string.IsNullOrWhiteSpace(notificationId))
            {
                query += (query.Length == 0 ? string.Empty : "&")
                    + $"notification={Uri.EscapeDataString(notificationId)}";
            }
            return query.Length == 0 ? route : $"{route}?{query}";
        }

        internal static NotificationRoute Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try
            {
                var uri = new Uri(json, UriKind.Absolute);
                if (!string.Equals(uri.Scheme, "novels", StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(uri.Host, "catalog", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                var segments = uri.AbsolutePath.Trim('/').Split('/');
                if (segments.Length != 2
                    || !string.Equals(segments[0], "story", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                var route = new NotificationRoute
                {
                    storyId = Uri.UnescapeDataString(segments[1]),
                };
                foreach (var pair in uri.Query.TrimStart('?').Split('&'))
                {
                    if (string.IsNullOrWhiteSpace(pair))
                        continue;
                    var parts = pair.Split(new[] {'='}, 2);
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                    if (string.Equals(key, "episode", StringComparison.OrdinalIgnoreCase))
                        route.episodeId = value;
                    else if (string.Equals(key, "notification", StringComparison.OrdinalIgnoreCase))
                        route.notificationId = value;
                }
                return route.IsValid ? route : null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
