using System;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Cache
{
    public class Entity : BaseDisposable
    {
        private readonly string _localPath;

        public Entity(string persistentDataPath)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
                throw new ArgumentException(
                    "Persistent data path must not be empty.",
                    nameof(persistentDataPath));

            _localPath = Path.Combine(persistentDataPath, "CachedFiles");
        }

        public async UniTask<AssetBundle> BundleFromCache(string path)
        {
            var rawData = ReadBytes(path);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        public async UniTask<AssetBundle> BundleToCache(string path, byte[] data)
        {
            WriteBytes(path, data);
            return await BundleFromCache(path);
        }
        
        public string TextFromCache(string path)
        {
            return Encoding.UTF8.GetString(ReadBytes(path));
        }

        public string TextToCache(string path, string data)
        {
            WriteBytes(path, Encoding.UTF8.GetBytes(data));
            return TextFromCache(path);
        }

        public byte[] ReadBytes(string path)
        {
            return File.ReadAllBytes(ConvertLocalPath(path));
        }

        public void WriteBytes(string path, byte[] data)
        {
            var file = ConvertLocalPath(path);
            var temporaryFile = $"{file}.{Guid.NewGuid():N}.tmp";

            try
            {
                File.WriteAllBytes(temporaryFile, data);
                if (File.Exists(file))
                    File.Replace(temporaryFile, file, null);
                else
                    File.Move(temporaryFile, file);
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }

        public bool Exists(string path)
        {
            return File.Exists(ConvertLocalPath(path, false));
        }

        public void Delete(string path)
        {
            var file = ConvertLocalPath(path, false);
            if (File.Exists(file))
                File.Delete(file);
        }

        public void PruneDirectory(string directoryPath, string keepFileName)
        {
            var directory = ConvertLocalPath(directoryPath, false);
            if (!Directory.Exists(directory))
                return;

            foreach (var file in Directory.GetFiles(directory))
            {
                if (!string.Equals(
                        Path.GetFileName(file),
                        keepFileName,
                        StringComparison.Ordinal))
                {
                    File.Delete(file);
                }
            }
        }

        public byte[] ByteArrayFromCash(string path)
        {
            return ReadBytes(path);
        }

        public void ByteArrayToCash(byte[] data, string path)
        {
            WriteBytes(path, data);
        }

        public string ConvertLocalPath(string path)
        {
            return ConvertLocalPath(path, true);
        }

        private string ConvertLocalPath(string path, bool createDirectory)
        {
            var relativePath = path
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var result = Path.Combine(_localPath, relativePath);

            if (createDirectory)
            {
                var directory = Path.GetDirectoryName(result);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
            }

            return result;
        }
    }
}
