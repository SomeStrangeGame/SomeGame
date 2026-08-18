using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Editor
{
    internal sealed class ContentBuildWorkspace : IDisposable
    {
        private sealed class Entry
        {
            internal Entry(string staged, string destination, string backup)
            {
                Staged = staged;
                Destination = destination;
                Backup = backup;
            }

            internal string Staged { get; }
            internal string Destination { get; }
            internal string Backup { get; }
            internal bool HasBackup { get; set; }
            internal bool Installed { get; set; }
        }

        private readonly string _projectPath;
        private readonly string _root;
        private bool _committed;
        private bool _retainWorkspace;

        internal ContentBuildWorkspace()
        {
            _projectPath = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project path cannot be resolved.");
            _root = Path.Combine(
                _projectPath,
                "Library",
                $"NovelContentWorkspace-{Guid.NewGuid():N}");
            RemoteRoot = Path.Combine(_root, "Remote");
            PublishRoot = Path.Combine(_root, "Publish");
            PlayerSeedRoot = Path.Combine(_root, "PlayerSeed");
            Directory.CreateDirectory(_root);
        }

        internal string RemoteRoot { get; }
        internal string PublishRoot { get; }
        internal string PlayerSeedRoot { get; }

        internal string PlayerSeedPath(string platform) =>
            Path.Combine(PlayerSeedRoot, platform);

        internal void Commit(
            NovelContentBuildProfile profile,
            IReadOnlyList<ContentBuildResult> results)
        {
            if (_committed)
                throw new InvalidOperationException("Content workspace is already committed.");
            var entries = BuildEntries(profile, results);
            foreach (var entry in entries)
            {
                if (!Directory.Exists(entry.Staged))
                    throw new DirectoryNotFoundException(entry.Staged);
            }

            try
            {
                foreach (var entry in entries)
                {
                    if (!Directory.Exists(entry.Destination))
                        continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.Backup));
                    Directory.Move(entry.Destination, entry.Backup);
                    entry.HasBackup = true;
                }
                foreach (var entry in entries)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.Destination));
                    Directory.Move(entry.Staged, entry.Destination);
                    entry.Installed = true;
                }
                foreach (var result in results)
                {
                    result.RemotePath = Path.Combine(
                        Application.streamingAssetsPath,
                        "Remote",
                        result.Platform);
                    result.PublishPath = GetProjectDestination(profile.PublishRoot);
                    result.PlayerSeedPath = profile.DeliveryMode
                        == Bundles.ContentDeliveryMode.Remote
                            ? null
                            : GetProjectDestination(
                                profile.PlayerSeedRoot,
                                result.Platform);
                }
                _committed = true;
                DeleteBackups();
            }
            catch (Exception commitFailure)
            {
                try
                {
                    Rollback(entries);
                }
                catch (Exception rollbackFailure)
                {
                    _retainWorkspace = true;
                    throw new AggregateException(
                        $"Content commit and rollback failed. Recovery files remain at '{_root}'.",
                        commitFailure,
                        rollbackFailure);
                }
                throw;
            }
        }

        public void Dispose()
        {
            if (_retainWorkspace || !Directory.Exists(_root))
                return;
            try
            {
                Directory.Delete(_root, true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Content workspace cleanup failed for '{_root}': {exception.Message}");
            }
        }

        private IReadOnlyList<Entry> BuildEntries(
            NovelContentBuildProfile profile,
            IReadOnlyList<ContentBuildResult> results)
        {
            var staged = new List<(string path, string destination)>
            {
                (RemoteRoot, Path.Combine(Application.streamingAssetsPath, "Remote")),
                (PublishRoot, GetProjectDestination(profile.PublishRoot)),
            };
            foreach (var result in results)
            {
                if (profile.DeliveryMode != Bundles.ContentDeliveryMode.Remote)
                {
                    staged.Add((
                        PlayerSeedPath(result.Platform),
                        GetProjectDestination(profile.PlayerSeedRoot, result.Platform)));
                }
            }
            return staged.Select((value, index) => new Entry(
                    Path.GetFullPath(value.path),
                    Path.GetFullPath(value.destination),
                    Path.Combine(_root, "Backups", index.ToString())))
                .ToArray();
        }

        private string GetProjectDestination(string root, string platform) =>
            Path.Combine(
                _projectPath,
                root.Replace('/', Path.DirectorySeparatorChar),
                platform);

        private string GetProjectDestination(string root) =>
            Path.Combine(
                _projectPath,
                root.Replace('/', Path.DirectorySeparatorChar));

        private void DeleteBackups()
        {
            var backupRoot = Path.Combine(_root, "Backups");
            if (!Directory.Exists(backupRoot))
                return;
            try
            {
                Directory.Delete(backupRoot, true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Content backup cleanup failed for '{backupRoot}': {exception.Message}");
            }
        }

        private static void Rollback(IReadOnlyList<Entry> entries)
        {
            Exception failure = null;
            for (var index = entries.Count - 1; index >= 0; index--)
            {
                var entry = entries[index];
                try
                {
                    if (entry.Installed && Directory.Exists(entry.Destination))
                        Directory.Delete(entry.Destination, true);
                    if (entry.HasBackup && Directory.Exists(entry.Backup))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(entry.Destination));
                        Directory.Move(entry.Backup, entry.Destination);
                    }
                }
                catch (Exception exception)
                {
                    failure ??= exception;
                }
            }
            if (failure != null)
                throw new InvalidOperationException("Content workspace rollback failed.", failure);
        }
    }
}
