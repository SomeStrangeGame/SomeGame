using System;
using System.IO;
using System.Security.Cryptography;

namespace Bundles
{
    public static class ContentHash
    {
        public static string ComputeSha256(string path)
        {
            using var stream = File.OpenRead(path);
            return ComputeSha256(stream);
        }

        public static string ComputeSha256(byte[] data)
        {
            using var stream = new MemoryStream(data, false);
            return ComputeSha256(stream);
        }

        private static string ComputeSha256(Stream stream)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
