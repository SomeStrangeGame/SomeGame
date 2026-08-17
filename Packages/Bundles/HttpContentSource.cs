using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Bundles
{
    public sealed class HttpContentSource : IContentSource
    {
        private readonly Uri _baseUri;
        private readonly CancellationToken _cancellationToken;

        public HttpContentSource(
            string baseUrl,
            CancellationToken cancellationToken)
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
            _cancellationToken = cancellationToken;
        }

        public string GetUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Content path must not be empty.", nameof(relativePath));
            return new Uri(_baseUri, relativePath.TrimStart('/')).AbsoluteUri;
        }

        public async UniTask<string> DownloadText(string path)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                await Send(request);
                return request.downloadHandler.text;
            }
        }

        public async UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes = null)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                request.downloadHandler = new DownloadHandlerFile(destinationPath, true);
                await Send(request, onDownloadedBytes);
            }
        }

        private async UniTask Send(
            UnityWebRequest request,
            Action<long> onDownloadedBytes = null)
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                onDownloadedBytes?.Invoke((long)request.downloadedBytes);
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new ContentSourceException(
                    $"Request failed [{request.responseCode}] {request.url}: {request.error}");
            }
            onDownloadedBytes?.Invoke((long)request.downloadedBytes);
        }
    }
}
