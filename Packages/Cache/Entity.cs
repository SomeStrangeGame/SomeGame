using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
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
            return await AssetBundle.LoadFromFileAsync(GetLocalPath(path, false));
        }

        public string TextFromCache(string path)
        {
            return Encoding.UTF8.GetString(ReadBytes(path));
        }

        public string TextToCache(string path, string data)
        {
            WriteBytes(path, Encoding.UTF8.GetBytes(data));
            return data;
        }

        public byte[] ReadBytes(string path)
        {
            return File.ReadAllBytes(GetLocalPath(path));
        }

        public void WriteBytes(string path, byte[] data)
        {
            var file = GetLocalPath(path);
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
            return File.Exists(GetLocalPath(path, false));
        }

        public void Delete(string path)
        {
            var file = GetLocalPath(path, false);
            if (File.Exists(file))
                File.Delete(file);
        }

        public void PruneDirectory(string directoryPath, string keepFileName)
        {
            var directory = GetLocalPath(directoryPath, false);
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

        public string ConvertLocalPath(string path)
        {
            return GetLocalPath(path);
        }

        public string GetLocalPath(string path, bool createDirectory = true)
        {
            if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
                throw new ArgumentException("Cache path must be relative.", nameof(path));
            var normalized = path.Replace(
                Path.AltDirectorySeparatorChar,
                Path.DirectorySeparatorChar);
            foreach (var part in normalized.Split(Path.DirectorySeparatorChar))
            {
                if (part == ".." || part == ".")
                    throw new ArgumentException(
                        "Cache path traversal is not allowed.",
                        nameof(path));
            }
            var root = Path.GetFullPath(_localPath)
                .TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            var result = Path.GetFullPath(Path.Combine(root, normalized));
            if (!result.StartsWith(root, StringComparison.Ordinal))
                throw new ArgumentException(
                    "Cache path escapes the cache root.",
                    nameof(path));

            if (createDirectory)
            {
                var directory = Path.GetDirectoryName(result);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
            }

            return result;
        }

        public string CreateTemporaryPath(string finalPath)
        {
            var file = GetLocalPath(finalPath);
            return $"{file}.{Guid.NewGuid():N}.tmp";
        }

        public string CreateTemporaryFile(string directoryPath)
        {
            return GetLocalPath(
                $"{directoryPath}/{Guid.NewGuid():N}.tmp");
        }

        public void PruneTemporaryFiles(string directoryPath, DateTime olderThanUtc)
        {
            var directory = GetLocalPath(directoryPath, false);
            if (!Directory.Exists(directory))
                return;
            foreach (var file in new DirectoryInfo(directory).GetFiles("*.tmp"))
            {
                if (file.LastWriteTimeUtc < olderThanUtc)
                    file.Delete();
            }
        }

        public void CommitTemporaryFile(string temporaryPath, string finalPath)
        {
            var file = GetLocalPath(finalPath);
            if (File.Exists(file))
                File.Replace(temporaryPath, file, null);
            else
                File.Move(temporaryPath, file);
        }

        public void Touch(string path)
        {
            File.SetLastAccessTimeUtc(GetLocalPath(path, false), DateTime.UtcNow);
        }

        public void PruneBySize(
            string directoryPath,
            long maximumBytes,
            string protectedPath = null)
        {
            PruneBySize(
                directoryPath,
                maximumBytes,
                string.IsNullOrWhiteSpace(protectedPath)
                    ? Array.Empty<string>()
                    : new[] { protectedPath });
        }

        public void PruneBySize(
            string directoryPath,
            long maximumBytes,
            IEnumerable<string> protectedPaths)
        {
            var directory = GetLocalPath(directoryPath, false);
            if (!Directory.Exists(directory) || maximumBytes <= 0)
                return;
            var protectedFiles = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in protectedPaths ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(path))
                    protectedFiles.Add(GetLocalPath(path, false));
            }
            var files = new DirectoryInfo(directory)
                .GetFiles("*", SearchOption.AllDirectories);
            long size = 0;
            foreach (var file in files)
                size += file.Length;
            Array.Sort(files, (left, right) =>
                left.LastAccessTimeUtc.CompareTo(right.LastAccessTimeUtc));
            foreach (var file in files)
            {
                if (size <= maximumBytes)
                    break;
                if (protectedFiles.Contains(file.FullName))
                    continue;
                size -= file.Length;
                file.Delete();
            }
        }

        public long PruneForAvailableSpace(
            string directoryPath,
            long requiredAvailableBytes,
            IEnumerable<string> protectedPaths)
        {
            var available = GetAvailableFreeSpace();
            if (!available.HasValue || available.Value >= requiredAvailableBytes)
                return 0L;
            var directory = GetLocalPath(directoryPath, false);
            if (!Directory.Exists(directory))
                return 0L;
            var protectedFiles = new HashSet<string>(StringComparer.Ordinal);
            foreach (var path in protectedPaths ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(path))
                    protectedFiles.Add(GetLocalPath(path, false));
            }
            var files = new DirectoryInfo(directory)
                .GetFiles("*", SearchOption.AllDirectories);
            Array.Sort(files, (left, right) =>
                left.LastAccessTimeUtc.CompareTo(right.LastAccessTimeUtc));
            var reclaimed = 0L;
            foreach (var file in files)
            {
                if (available.Value + reclaimed >= requiredAvailableBytes)
                    break;
                if (protectedFiles.Contains(file.FullName))
                    continue;
                reclaimed += file.Length;
                file.Delete();
            }
            return reclaimed;
        }

        public long? GetAvailableFreeSpace()
        {
            try
            {
                var root = Path.GetPathRoot(Path.GetFullPath(_localPath));
                return string.IsNullOrWhiteSpace(root)
                    ? null
                    : new DriveInfo(root).AvailableFreeSpace;
            }
            catch
            {
                return null;
            }
        }
    }
}
