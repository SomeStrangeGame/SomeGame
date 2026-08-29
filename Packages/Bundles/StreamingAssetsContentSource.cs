using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class StreamingAssetsContentSource : IContentSource
    {
        private readonly string _root;
        private readonly ContentRequestRunner _requests;

        public StreamingAssetsContentSource(
            string root,
            CancellationToken cancellationToken,
            ContentRequestPolicy policy = null)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("Content root must not be empty.", nameof(root));
            _root = root.TrimEnd('/');
            _requests = new ContentRequestRunner(
                cancellationToken,
                policy ?? ContentRequestPolicy.LocalDefault);
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) =>
            payloadPath;

        public string GetUrl(string relativePath) =>
            $"{_root}/{Normalize(relativePath)}";

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

        private static string Normalize(string path)
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
            return relative;
        }
    }
}
