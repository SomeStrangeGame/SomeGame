using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class HttpContentSource : IContentSource
    {
        private readonly Uri _baseUri;
        private readonly ContentRequestRunner _requests;

        public HttpContentSource(
            string baseUrl,
            CancellationToken cancellationToken,
            ContentRequestPolicy policy = null)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp
                    && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException(
                    "Remote content base URL must be an absolute HTTP(S) URL.",
                    nameof(baseUrl));
            }

            _baseUri = new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
            _requests = new ContentRequestRunner(
                cancellationToken,
                policy ?? ContentRequestPolicy.RemoteDefault);
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) =>
            payloadPath;

        public string GetUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Content path must not be empty.", nameof(relativePath));
            return new Uri(_baseUri, relativePath.TrimStart('/')).AbsoluteUri;
        }

        public UniTask<string> DownloadText(
            string path,
            CancellationToken cancellationToken) =>
            _requests.DownloadText(GetUrl(path), cancellationToken);

        public UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes,
            CancellationToken cancellationToken)
            => _requests.DownloadFile(
                GetUrl(path),
                destinationPath,
                onDownloadedBytes,
                cancellationToken);
    }
}
