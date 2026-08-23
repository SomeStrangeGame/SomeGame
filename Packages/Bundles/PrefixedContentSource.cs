using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class PrefixedContentSource : IContentSource
    {
        private readonly IContentSource _source;
        private readonly string _prefix;

        public PrefixedContentSource(IContentSource source, string prefix)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Content prefix must not be empty.", nameof(prefix));
            _prefix = prefix.Trim().Trim('/');
            if (_prefix.Length == 0 || _prefix.Contains(".."))
                throw new ArgumentException("Content prefix is invalid.", nameof(prefix));
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) =>
            _source.ResolveFilePayloadPath(logicalPath, payloadPath);

        public string GetUrl(string relativePath) =>
            _source.GetUrl(Prefix(relativePath));

        public UniTask<string> DownloadText(
            string path,
            CancellationToken cancellationToken) =>
            _source.DownloadText(Prefix(path), cancellationToken);

        public UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes,
            CancellationToken cancellationToken) =>
            _source.DownloadFile(
                Prefix(path),
                destinationPath,
                onDownloadedBytes,
                cancellationToken);

        private string Prefix(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Content path must not be empty.", nameof(path));
            var relative = path.Trim().TrimStart('/');
            var segments = relative.Split('/');
            foreach (var segment in segments)
            {
                if (segment.Length == 0 || segment == "." || segment == "..")
                {
                    throw new ArgumentException(
                        "Content path must be a normalized relative path.",
                        nameof(path));
                }
            }
            return $"{_prefix}/{relative}";
        }
    }
}
