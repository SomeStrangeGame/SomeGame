using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public interface IContentSource
    {
        string ResolveFilePayloadPath(string logicalPath, string payloadPath);
        string GetUrl(string relativePath);
        UniTask<string> DownloadText(string path, CancellationToken cancellationToken);
        UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes,
            CancellationToken cancellationToken);
    }
}
