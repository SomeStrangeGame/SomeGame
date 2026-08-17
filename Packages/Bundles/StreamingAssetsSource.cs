using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Bundles
{
    public sealed class StreamingAssetsSource : IContentSource
    {
        private readonly CancellationToken _cancellationToken;

        public StreamingAssetsSource(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public string GetUrl(string relativePath)
        {
            var path = $"{Application.streamingAssetsPath}/{relativePath}";
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return new Uri(path).AbsoluteUri;
#else
            return path;
#endif
        }

        public async UniTask<string> DownloadText(string path)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                await Send(request);
                return request.downloadHandler.text;
            }
        }

        public async UniTask DownloadFile(string path, string destinationPath)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                request.downloadHandler = new DownloadHandlerFile(
                    destinationPath,
                    true);
                await Send(request);
            }
        }

        private async UniTask Send(UnityWebRequest request)
        {
            await request.SendWebRequest().WithCancellation(_cancellationToken);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new ContentSourceException(
                    $"Request failed [{request.responseCode}] {request.url}: {request.error}");
            }
        }
    }
}
