using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Bundles
{
    internal sealed class StreamingAssetsSource
    {
        private readonly CancellationToken _cancellationToken;

        internal StreamingAssetsSource(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal string GetUrl(string relativePath)
        {
            var path = $"{Application.streamingAssetsPath}/{relativePath}";
#if UNITY_EDITOR_OSX
            return new Uri(path).AbsoluteUri;
#else
            return path;
#endif
        }

        internal async UniTask<byte[]> DownloadBytes(string path)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                await Send(request);
                return request.downloadHandler.data;
            }
        }

        internal async UniTask<string> DownloadText(string path)
        {
            using (var request = UnityWebRequest.Get(GetUrl(path)))
            {
                await Send(request);
                return request.downloadHandler.text;
            }
        }

        private async UniTask Send(UnityWebRequest request)
        {
            await request.SendWebRequest().WithCancellation(_cancellationToken);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"Request failed [{request.responseCode}] {request.url}: {request.error}");
            }
        }
    }
}
