using System;
using System.Threading;
using Bundles;
using Cysharp.Threading.Tasks;

namespace Novels.WebPlayer
{
    // Web release directory contains release.json, bundle directories and Files/.
    public sealed class WebStoryContentSource : IContentSource
    {
        private readonly HttpContentSource _http;
        private readonly string _manifest;
        public WebStoryContentSource(string pageUrl, WebPlayerLaunchConfiguration config,
            CancellationToken cancellation)
        {
            if (!config.TryValidate(out var error)) throw new ArgumentException(error);
            var manifest = new Uri(new Uri(pageUrl), config.manifestUrl);
            var page = new Uri(pageUrl);
            if (manifest.Scheme != page.Scheme || manifest.Authority != page.Authority)
                throw new ArgumentException("Content must have the same origin as the player.");
            _manifest = Uri.UnescapeDataString(manifest.Segments[manifest.Segments.Length - 1]);
            _http = new HttpContentSource(new Uri(manifest, ".").AbsoluteUri, cancellation);
        }
        private string Map(string path)
        {
            const string prefix = "Remote/WebGL/";
            if (path == prefix + "release.json") return _manifest;
            if (path.StartsWith(prefix, StringComparison.Ordinal)) path = path.Substring(prefix.Length);
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith("/")
                || path.Contains("..") || path.Contains(":") || path.Contains("\\")
                || path.Contains("%") || path.Contains("?") || path.Contains("#"))
                throw new ArgumentException("Invalid relative content path.");
            return path;
        }
        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) => payloadPath;
        public string GetUrl(string path) => _http.GetUrl(Map(path));
        public UniTask<string> DownloadText(string path, CancellationToken token) =>
            _http.DownloadText(Map(path), token);
        public UniTask DownloadFile(string path, string destination, Action<long> progress, CancellationToken token) =>
            _http.DownloadFile(Map(path), destination, progress, token);
    }
}
