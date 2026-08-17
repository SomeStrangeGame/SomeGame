using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    public sealed class StreamingAssetsSource : IContentSource
    {
        private readonly ContentRequestRunner _requests;

        public StreamingAssetsSource(CancellationToken cancellationToken)
        {
            _requests = new ContentRequestRunner(
                cancellationToken,
                ContentRequestPolicy.LocalDefault);
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

        public UniTask<string> DownloadText(string path) =>
            _requests.DownloadText(GetUrl(path));

        public UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes = null)
            => _requests.DownloadFile(
                GetUrl(path),
                destinationPath,
                onDownloadedBytes);
    }
}
