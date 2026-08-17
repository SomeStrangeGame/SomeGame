using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Bundles
{
    internal sealed class ContentRequestRunner
    {
        private readonly CancellationToken _cancellationToken;

        internal ContentRequestRunner(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<string> DownloadText(string url)
        {
            using var request = UnityWebRequest.Get(url);
            await Send(request);
            return request.downloadHandler.text;
        }

        internal async UniTask DownloadFile(
            string url,
            string destinationPath,
            Action<long> onDownloadedBytes)
        {
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(destinationPath, true);
            await Send(request, onDownloadedBytes);
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
