using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class ContentBuildTransaction
    {
        internal static IReadOnlyList<ContentBuildResult> Build(
            NovelContentBuildProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            var projectPath = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project path cannot be resolved.");
            var destinations = GetDestinations(projectPath, profile);
            using var transaction = new DirectoryTransaction(projectPath, destinations);
            transaction.Begin();
            try
            {
                var results = AssetBundleBuildPipeline.Build(profile);
                NovelContentValidator.ValidateBuiltOutputOrThrow();
                foreach (var result in results)
                {
                    ContentPublishArtifactBuilder.Build(result, profile);
                    if (profile.DeliveryMode != Bundles.ContentDeliveryMode.Remote)
                        PlayerContentSeedBuilder.Build(result, profile);
                }
                transaction.Commit();
                return results;
            }
            catch
            {
                transaction.Rollback();
                AssetDatabase.Refresh();
                throw;
            }
        }

        private static IReadOnlyList<string> GetDestinations(
            string projectPath,
            NovelContentBuildProfile profile)
        {
            var result = new List<string>
            {
                Path.Combine(Application.streamingAssetsPath, "Remote"),
            };
            foreach (var target in profile.Targets)
            {
                var platform = AssetBundleBuildPipeline.GetPlatformName(target);
                result.Add(Path.Combine(
                    projectPath,
                    profile.PublishRoot.Replace('/', Path.DirectorySeparatorChar),
                    platform));
                if (profile.DeliveryMode != Bundles.ContentDeliveryMode.Remote)
                {
                    result.Add(Path.Combine(
                        projectPath,
                        profile.PlayerSeedRoot.Replace('/', Path.DirectorySeparatorChar),
                        platform));
                }
            }
            return result;
        }

        private sealed class DirectoryTransaction : IDisposable
        {
            private readonly string _backupRoot;
            private readonly IReadOnlyList<Entry> _entries;
            private bool _begun;
            private bool _completed;

            internal DirectoryTransaction(
                string projectPath,
                IReadOnlyList<string> destinations)
            {
                _backupRoot = Path.Combine(
                    projectPath,
                    "Library",
                    $"NovelContentTransaction-{Guid.NewGuid():N}");
                var entries = new List<Entry>(destinations.Count);
                for (var index = 0; index < destinations.Count; index++)
                {
                    entries.Add(new Entry(
                        Path.GetFullPath(destinations[index]),
                        Path.Combine(_backupRoot, index.ToString())));
                }
                _entries = entries;
            }

            internal void Begin()
            {
                if (_begun)
                    throw new InvalidOperationException("Content transaction already began.");
                _begun = true;
                Directory.CreateDirectory(_backupRoot);
                try
                {
                    foreach (var entry in _entries)
                    {
                        if (!Directory.Exists(entry.Destination))
                            continue;
                        Directory.CreateDirectory(Path.GetDirectoryName(entry.Backup));
                        Directory.Move(entry.Destination, entry.Backup);
                        entry.HasBackup = true;
                    }
                }
                catch
                {
                    Rollback();
                    throw;
                }
            }

            internal void Commit()
            {
                if (!_begun || _completed)
                    throw new InvalidOperationException("Content transaction is not active.");
                if (Directory.Exists(_backupRoot))
                    Directory.Delete(_backupRoot, true);
                _completed = true;
            }

            internal void Rollback()
            {
                if (!_begun || _completed)
                    return;
                Exception failure = null;
                for (var index = _entries.Count - 1; index >= 0; index--)
                {
                    var entry = _entries[index];
                    try
                    {
                        if (Directory.Exists(entry.Destination))
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
                _completed = true;
                if (Directory.Exists(_backupRoot))
                    Directory.Delete(_backupRoot, true);
                if (failure != null)
                    throw new InvalidOperationException(
                        "Content transaction rollback failed.",
                        failure);
            }

            public void Dispose()
            {
                Rollback();
            }

            private sealed class Entry
            {
                internal Entry(string destination, string backup)
                {
                    Destination = destination;
                    Backup = backup;
                }

                internal string Destination { get; }
                internal string Backup { get; }
                internal bool HasBackup { get; set; }
            }
        }
    }
}
