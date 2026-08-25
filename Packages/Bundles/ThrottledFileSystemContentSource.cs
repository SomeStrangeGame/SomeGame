using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class ThrottledFileSystemContentSource : IContentSource
    {
        private const int _bufferSize = 64 * 1024;
        private readonly string _root;
        private readonly double _bytesPerSecond;

        public ThrottledFileSystemContentSource(string root, double megabitsPerSecond)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("Content root must not be empty.", nameof(root));
            if (megabitsPerSecond <= 0d)
                throw new ArgumentOutOfRangeException(nameof(megabitsPerSecond));
            _root = Path.GetFullPath(root);
            _bytesPerSecond = megabitsPerSecond * 1_000_000d / 8d;
        }

        public string ResolveFilePayloadPath(string logicalPath, string payloadPath) =>
            payloadPath;

        public string GetUrl(string relativePath) =>
            new Uri(ResolvePath(relativePath)).AbsoluteUri;

        public async UniTask<string> DownloadText(
            string path,
            CancellationToken cancellationToken)
        {
            var source = ResolvePath(path);
            await DelayForBytes(new FileInfo(source).Length, cancellationToken);
            return await File.ReadAllTextAsync(source, cancellationToken);
        }

        public async UniTask DownloadFile(
            string path,
            string destinationPath,
            Action<long> onDownloadedBytes,
            CancellationToken cancellationToken)
        {
            var sourcePath = ResolvePath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
            await using var source = new FileStream(
                sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
            await using var destination = new FileStream(
                destinationPath, FileMode.Create, FileAccess.Write, FileShare.None,
                _bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var buffer = new byte[_bufferSize];
            long completed = 0;
            while (true)
            {
                var read = await source.ReadAsync(
                    buffer, 0, buffer.Length, cancellationToken);
                if (read <= 0)
                    break;
                await destination.WriteAsync(buffer, 0, read, cancellationToken);
                completed += read;
                onDownloadedBytes?.Invoke(completed);
                await DelayForBytes(read, cancellationToken);
            }
        }

        public static bool TryParseMegabits(string value, out double result) =>
            double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out result)
            && result > 0d;

        private UniTask DelayForBytes(long bytes, CancellationToken cancellationToken)
        {
            var milliseconds = (int)Math.Ceiling(bytes / _bytesPerSecond * 1000d);
            return milliseconds <= 0
                ? UniTask.CompletedTask
                : UniTask.Delay(milliseconds, cancellationToken: cancellationToken);
        }

        private string ResolvePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Content path must not be empty.", nameof(relativePath));
            var combined = Path.GetFullPath(Path.Combine(
                _root,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            var rootPrefix = _root.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!combined.StartsWith(rootPrefix, StringComparison.Ordinal)
                && !string.Equals(combined, _root, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Content path must remain inside the configured root.",
                    nameof(relativePath));
            }
            return combined;
        }
    }
}
