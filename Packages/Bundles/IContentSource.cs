using System;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public interface IContentSource
    {
        string GetUrl(string relativePath);
        UniTask<string> DownloadText(string path);
        UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes = null);
    }
}
