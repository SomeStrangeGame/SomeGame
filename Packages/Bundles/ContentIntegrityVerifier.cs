using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class ContentIntegrityVerifier
    {
        private readonly HashSet<string> _verified = new(StringComparer.Ordinal);
        private readonly object _gate = new();
        private readonly CancellationToken _cancellationToken;

        internal ContentIntegrityVerifier(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal async UniTask VerifyAsync(
            string name,
            long expectedSize,
            string expectedSha256,
            string path,
            bool verifyIntegrity)
        {
            var key = GetKey(path, expectedSize, expectedSha256, verifyIntegrity);
            lock (_gate)
            {
                if (_verified.Contains(key) && File.Exists(path))
                    return;
                _verified.Remove(key);
            }

            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                _cancellationToken.ThrowIfCancellationRequested();
                Verify(name, expectedSize, expectedSha256, path, verifyIntegrity);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            _cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
                _verified.Add(key);
        }

        internal void Trust(
            string path,
            long expectedSize,
            string expectedSha256,
            bool verifyIntegrity)
        {
            lock (_gate)
            {
                _verified.Add(GetKey(
                    path,
                    expectedSize,
                    expectedSha256,
                    verifyIntegrity));
            }
        }

        private static void Verify(
            string name,
            long expectedSize,
            string expectedSha256,
            string path,
            bool verifyIntegrity)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Cached content file is missing.", path);
            if (!verifyIntegrity)
                return;
            var file = new FileInfo(path);
            if (file.Length != expectedSize)
            {
                throw new ContentIntegrityException(
                    $"Content '{name}' size mismatch. Expected "
                    + $"{expectedSize}, got {file.Length}.");
            }

            using var sha = SHA256.Create();
            using var stream = File.OpenRead(path);
            var actual = BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty);
            if (!string.Equals(
                    actual,
                    expectedSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ContentIntegrityException(
                    $"Content '{name}' SHA-256 mismatch.");
            }
        }

        private static string GetKey(
            string path,
            long size,
            string sha256,
            bool verifyIntegrity) =>
            $"{Path.GetFullPath(path)}|{size}|{sha256}|{verifyIntegrity}";
    }
}
