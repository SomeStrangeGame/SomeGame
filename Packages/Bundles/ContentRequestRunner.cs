using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Bundles
{
    internal sealed class ContentRequestRunner
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ContentRequestPolicy _policy;

        internal ContentRequestRunner(
            CancellationToken cancellationToken,
            ContentRequestPolicy policy)
        {
            _cancellationToken = cancellationToken;
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        internal async UniTask<string> DownloadText(string url)
        {
            for (var attempt = 1; ; attempt++)
            {
                using var request = UnityWebRequest.Get(url);
                try
                {
                    await Send(request);
                    return request.downloadHandler.text;
                }
                catch (ContentSourceException exception)
                    when (ShouldRetry(exception, attempt))
                {
                    await WaitBeforeRetry(attempt);
                }
            }
        }

        internal async UniTask DownloadFile(
            string url,
            string destinationPath,
            Action<long> onDownloadedBytes)
        {
            for (var attempt = 1; ; attempt++)
            {
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);
                using var request = UnityWebRequest.Get(url);
                request.downloadHandler = new DownloadHandlerFile(destinationPath, false);
                try
                {
                    await Send(request, onDownloadedBytes);
                    return;
                }
                catch (ContentSourceException exception)
                    when (ShouldRetry(exception, attempt))
                {
                    await WaitBeforeRetry(attempt);
                }
                catch
                {
                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);
                    throw;
                }
            }
        }

        private async UniTask Send(
            UnityWebRequest request,
            Action<long> onDownloadedBytes = null)
        {
            if (_policy.TimeoutSeconds > 0)
                request.timeout = _policy.TimeoutSeconds;
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
                    $"Request failed [{request.responseCode}] {request.url}: {request.error}",
                    GetFailureKind(request),
                    request.responseCode);
            }
            onDownloadedBytes?.Invoke((long)request.downloadedBytes);
        }

        private bool ShouldRetry(ContentSourceException exception, int attempt) =>
            exception.IsTransient && attempt < _policy.MaximumAttempts;

        private async UniTask WaitBeforeRetry(int failedAttempt)
        {
            var delay = _policy.GetRetryDelayMilliseconds(failedAttempt);
            if (delay <= 0)
                return;
            await UniTask.Delay(
                delay,
                cancellationToken: _cancellationToken);
        }

        private static ContentSourceFailureKind GetFailureKind(UnityWebRequest request)
        {
            if (request.responseCode == 404)
                return ContentSourceFailureKind.NotFound;
            if (request.responseCode == 408)
                return ContentSourceFailureKind.Timeout;
            if (request.responseCode == 429)
                return ContentSourceFailureKind.RateLimited;
            if (request.responseCode >= 500)
                return ContentSourceFailureKind.Server;
            if (request.responseCode >= 400)
                return ContentSourceFailureKind.Client;
            if (request.error?.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
                return ContentSourceFailureKind.Timeout;
            if (request.result == UnityWebRequest.Result.ConnectionError)
                return ContentSourceFailureKind.Network;
            return ContentSourceFailureKind.Unknown;
        }
    }
}
