using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class BundleStore
    {
        private sealed class Record
        {
            internal AssetBundle Bundle;
            internal UniTask<AssetBundle> Loading;
            internal bool IsLoading;
            internal bool Persistent;
            internal int Leases;
        }

        private readonly Dictionary<string, Record> _records = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly IContentSource _source;
        private readonly Cache.Entity _cache;
        private readonly ContentReleaseProvider _releases;
        private readonly ContentIntegrityVerifier _integrity;
        private readonly BundledAssetCache _assets;
        private readonly string _platform;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;

        internal BundleStore(
            IContentSource source,
            Cache.Entity cache,
            ContentReleaseProvider releases,
            ContentIntegrityVerifier integrity,
            string platform,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog)
        {
            _source = source;
            _cache = cache;
            _releases = releases;
            _integrity = integrity;
            _platform = platform;
            _cancellationToken = cancellationToken;
            _onLog = onLog;
            _assets = new BundledAssetCache(cancellationToken);
        }

        internal UniTask<AssetBundle> GetPersistent(string bundleName) =>
            GetOrLoad(bundleName, true);

        internal async UniTask<AssetBundle> Acquire(string bundleName)
        {
            var bundle = await GetOrLoad(bundleName, false);
            _records[GetKey(bundleName)].Leases++;
            return bundle;
        }

        internal AssetBundle GetOwned(string bundleName) => GetLoaded(bundleName);

        internal UniTask<Sprite> GetSprite(string bundleName, string assetName)
        {
            var key = GetKey(bundleName);
            return _assets.GetSprite(
                bundleName,
                key,
                GetLoaded(bundleName),
                assetName);
        }

        internal UniTask<Sprite> TryGetSprite(string bundleName, string assetName)
        {
            var key = GetKey(bundleName);
            return _assets.TryGetSprite(
                key,
                GetLoaded(bundleName),
                assetName);
        }

        internal UniTask<T> GetScriptableObject<T>(
            string bundleName,
            string assetName)
            where T : ScriptableObject
        {
            var key = GetKey(bundleName);
            return _assets.GetScriptableObject<T>(
                bundleName,
                key,
                GetLoaded(bundleName),
                assetName);
        }

        internal UniTask<GameObject> GetPrefab(string bundleName, string assetName)
        {
            var key = GetKey(bundleName);
            return _assets.GetPrefab(
                bundleName,
                key,
                GetLoaded(bundleName),
                assetName);
        }

        internal string ResolveAssetName(string bundleName, string requestedName) =>
            _assets.Resolve(GetKey(bundleName), requestedName);

        internal void Release(IEnumerable<string> bundleNames)
        {
            foreach (var bundleName in bundleNames)
            {
                var key = GetKey(bundleName);
                if (!_records.TryGetValue(key, out var record))
                    continue;
                record.Leases = Math.Max(0, record.Leases - 1);
                if (record.Leases > 0 || record.Persistent)
                    continue;
                _records.Remove(key);
                record.Bundle?.Unload(false);
                _assets.Remove(key);
            }
        }

        internal void Clear()
        {
            foreach (var record in _records.Values)
                record.Bundle?.Unload(false);
            _records.Clear();
            _assets.Clear();
        }

        private async UniTask<AssetBundle> GetOrLoad(
            string bundleName,
            bool persistent)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
                throw new ContentConfigurationException("Bundle name is empty.");
            var key = GetKey(bundleName);
            if (!_records.TryGetValue(key, out var record))
            {
                record = new Record();
                _records.Add(key, record);
            }
            record.Persistent |= persistent;
            if (record.Bundle != null)
                return record.Bundle;
            if (!record.IsLoading)
            {
                record.IsLoading = true;
                record.Loading = Load(bundleName, key, record).Preserve();
            }
            return await record.Loading;
        }

        private async UniTask<AssetBundle> Load(
            string bundleName,
            string bundleKey,
            Record record)
        {
            try
            {
                var manifest = await GetManifest(bundleName);
                var version = manifest.version;
                var sourcePath = $"{bundleKey}/{version}";
                var localPath = _cache.GetLocalPath(sourcePath, false);
                try
                {
                    await _integrity.VerifyAsync(
                        bundleName,
                        manifest.size,
                        manifest.sha256,
                        localPath,
                        manifest.HasIntegrity);
                    record.Bundle = await _cache.BundleFromCache(sourcePath)
                        .AttachExternalCancellation(_cancellationToken);
                    if (record.Bundle == null)
                        throw new ContentIntegrityException(
                            $"Cached bundle '{sourcePath}' is invalid.");
                    _onLog?.Invoke((LogType.Log, $"Get local bundle from {sourcePath}"));
                }
                catch (OperationCanceledException)
                    when (_cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _onLog?.Invoke((
                        LogType.Warning,
                        $"Download bundle '{bundleName}' because its cache is invalid: "
                        + exception.Message));
                    var temporaryPath = _cache.CreateTemporaryPath(sourcePath);
                    try
                    {
                        await _source.DownloadFile(sourcePath, temporaryPath);
                        _cancellationToken.ThrowIfCancellationRequested();
                        await _integrity.VerifyAsync(
                            bundleName,
                            manifest.size,
                            manifest.sha256,
                            temporaryPath,
                            manifest.HasIntegrity);
                        _cache.CommitTemporaryFile(temporaryPath, sourcePath);
                        _integrity.Trust(
                            localPath,
                            manifest.size,
                            manifest.sha256,
                            manifest.HasIntegrity);
                    }
                    finally
                    {
                        if (File.Exists(temporaryPath))
                            File.Delete(temporaryPath);
                    }
                    record.Bundle = await _cache.BundleFromCache(sourcePath)
                        .AttachExternalCancellation(_cancellationToken);
                    if (record.Bundle == null)
                    {
                        throw new ContentIntegrityException(
                            $"Downloaded bundle '{sourcePath}' is invalid.");
                    }
                }

                _assets.Register(bundleKey, record.Bundle);
                _cache.PruneDirectory(bundleKey, version);
                _cache.TextToCache(GetVersionPointerPath(bundleName), version);
                if (manifest.HasIntegrity)
                {
                    _cache.TextToCache(
                        GetManifestPointerPath(bundleName),
                        JsonUtility.ToJson(manifest));
                }
                return record.Bundle;
            }
            finally
            {
                record.IsLoading = false;
            }
        }

        private async UniTask<BundleManifest> GetManifest(string bundleName)
        {
            var releaseEntry = _releases.Current?.FindBundle(bundleName);
            if (releaseEntry != null)
                return releaseEntry.ToManifest();
            if (_releases.Current != null)
            {
                throw new ContentIntegrityException(
                    $"Bundle '{bundleName}' is absent from release "
                    + $"'{_releases.Current.releaseId}'.");
            }
            var path = $"Remote/{_platform}/{bundleName}/manifest.json";
            try
            {
                var manifest = JsonUtility.FromJson<BundleManifest>(
                    await _source.DownloadText(path));
                ValidateManifest(bundleName, manifest);
                return manifest;
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var cachedPath = GetManifestPointerPath(bundleName);
                if (_cache.Exists(cachedPath))
                {
                    try
                    {
                        var cached = JsonUtility.FromJson<BundleManifest>(
                            _cache.TextFromCache(cachedPath));
                        ValidateManifest(bundleName, cached);
                        return cached;
                    }
                    catch (Exception cachedException)
                    {
                        _onLog?.Invoke((
                            LogType.Warning,
                            $"Cached manifest for '{bundleName}' is invalid: "
                            + cachedException.Message));
                    }
                }
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Use legacy bundle pointer for '{bundleName}': {exception.Message}"));
                return BundleManifest.Legacy(await GetVersion(bundleName));
            }
        }

        private async UniTask<string> GetVersion(string bundleName)
        {
            var path = $"Remote/{_platform}/{bundleName}/version.txt";
            try
            {
                var version = (await _source.DownloadText(path)).Trim();
                if (version.Length == 0)
                    throw new ContentIntegrityException(
                        $"Bundle version is empty for '{bundleName}'.");
                return version;
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var cachedPath = GetVersionPointerPath(bundleName);
                if (!_cache.Exists(cachedPath))
                {
                    throw new ContentSourceException(
                        $"Bundle version for '{bundleName}' is unavailable.",
                        exception);
                }
                var version = _cache.TextFromCache(cachedPath).Trim();
                if (version.Length == 0)
                    throw new ContentIntegrityException(
                        $"Cached bundle version is empty for '{bundleName}'.",
                        exception);
                return version;
            }
        }

        private static void ValidateManifest(string bundleName, BundleManifest manifest)
        {
            if (manifest == null)
                throw new ContentIntegrityException(
                    $"Integrity manifest for '{bundleName}' is missing.");
            if (manifest.HasIntegrity)
            {
                ContentReleaseValidator.ValidatePayload(
                    bundleName,
                    manifest.version,
                    manifest.size,
                    manifest.sha256);
            }
            else if (string.IsNullOrWhiteSpace(manifest.version))
            {
                throw new ContentIntegrityException(
                    $"Legacy version for '{bundleName}' is missing.");
            }
        }

        private AssetBundle GetLoaded(string bundleName)
        {
            var key = GetKey(bundleName);
            if (!_records.TryGetValue(key, out var record) || record.Bundle == null)
            {
                throw new ContentConfigurationException(
                    $"AssetBundle '{bundleName}' is not loaded.");
            }
            return record.Bundle;
        }

        private string GetKey(string bundleName) =>
            $"Remote/{_platform}/{bundleName}";

        private string GetVersionPointerPath(string bundleName) =>
            $"Remote/{_platform}/BundleVersions/{bundleName}.txt";

        private string GetManifestPointerPath(string bundleName) =>
            $"Remote/{_platform}/BundleManifests/{bundleName}.json";
    }
}
