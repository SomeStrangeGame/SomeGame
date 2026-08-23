using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class FileSystemContentSource : IContentSource
    {
        private readonly string _root;
        private readonly ContentRequestRunner _requests;

        public FileSystemContentSource(
            string root,
            CancellationToken cancellationToken,
            ContentRequestPolicy policy = null)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("Content root must not be empty.", nameof(root));
            _root = Path.GetFullPath(root);
            _requests = new ContentRequestRunner(
                cancellationToken,
                policy ?? ContentRequestPolicy.LocalDefault);
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) =>
            payloadPath;

        public string GetUrl(string relativePath) =>
            new Uri(ResolvePath(relativePath)).AbsoluteUri;

        public UniTask<string> DownloadText(
            string path,
            CancellationToken cancellationToken) =>
            _requests.DownloadText(GetUrl(path), cancellationToken);

        public UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes,
            CancellationToken cancellationToken) =>
            _requests.DownloadFile(
                GetUrl(path),
                destinationPath,
                onDownloadedBytes,
                cancellationToken);

        private string ResolvePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Content path must not be empty.", nameof(relativePath));
            var combined = Path.GetFullPath(Path.Combine(
                _root,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            var rootPrefix = _root.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!combined.StartsWith(rootPrefix, StringComparison.Ordinal)
                && !string.Equals(combined, _root, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Content path must remain inside the configured root.",
                    nameof(relativePath));
            }
            return combined;
        }
    }
}
