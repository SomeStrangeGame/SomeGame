using Cysharp.Threading.Tasks;

namespace Bundles
{
    public interface IContentSource
    {
        string GetUrl(string relativePath);
        UniTask<byte[]> DownloadBytes(string path);
        UniTask<string> DownloadText(string path);
    }
}
