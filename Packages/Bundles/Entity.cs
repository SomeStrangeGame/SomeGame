using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Bundles
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public IContentSource ContentSource;
            public string PersistentDataPath;
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            public Action<BundleFailure> OnFailure;
        }

        private readonly Dictionary<string, Sprite> _sprites = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _prefabs = new(StringComparer.OrdinalIgnoreCase);

        private readonly Cache.Entity _cache;
        private readonly Dictionary<string, BundleRecord> _bundles = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _assetNames = new(
            StringComparer.OrdinalIgnoreCase);

        private readonly IContentSource _source;
        private readonly MediaResolver _media;
        private ContentRelease _release;

        private const long _contentFileCacheLimit = 512L * 1024L * 1024L;

        private sealed class BundleRecord
        {
            internal AssetBundle Bundle;
            internal UniTask<AssetBundle> Loading;
            internal bool IsLoading;
            internal bool Persistent;
            internal int Leases;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _source = ctx.ContentSource
                ?? throw new ArgumentNullException(nameof(ctx.ContentSource));
            _cache = new Cache.Entity(ctx.PersistentDataPath).AddTo(this);
            _media = new MediaResolver(
                ResolveContentFileUrl,
                ctx.CancellationToken);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            ClearBundles();
        }

        public Scope CreateScope()
        {
            return new Scope(this);
        }

        private void ClearBundles()
        {
            foreach(var bundle in _bundles.Values)
                bundle.Bundle?.Unload(false);
            _bundles.Clear();
            _assetNames.Clear();
        }

        public async UniTask<Sprite> GetBundledSprite(string bundleName, string assetName)
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null) return null;
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_sprites.ContainsKey(assetKey))
            {
                _sprites[assetKey] = await assetBundle
                    .LoadAssetAsync<Sprite>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as Sprite;
            }
            return _sprites[assetKey];
        }

        public async UniTask<T> GetBundledSO<T>(string bundleName, string assetName) where T : ScriptableObject
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            var requestedName = assetName;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null)
            {
                ReportMissingAsset(bundleName, requestedName);
                return null;
            }
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_scriptableObjects.ContainsKey(assetKey))
            {
                _scriptableObjects[assetKey] = await assetBundle
                    .LoadAssetAsync<T>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as T;
            }
            return _scriptableObjects[assetKey] as T;
        }

        public async UniTask<GameObject> GetBundledPrefab(string bundleName, string assetName)
        {
            var assetBundle = GetLoadedBundle(bundleName);
            if (assetBundle == null) return null;
            if (string.IsNullOrEmpty(assetName)) return null;
            var requestedName = assetName;
            assetName = ResolveAssetName(bundleName, assetName);
            if (assetName == null)
            {
                ReportMissingAsset(bundleName, requestedName);
                return null;
            }
            var assetKey = GetAssetKey(bundleName, assetName);
            if (!_prefabs.ContainsKey(assetKey))
            {
                _prefabs[assetKey] = await assetBundle
                    .LoadAssetAsync<GameObject>(assetName)
                    .WithCancellation(_ctx.CancellationToken) as GameObject;
            }
            return _prefabs[assetKey];
        }

        public void ConfigureMedia(string prefix, MediaManifest manifest)
        {
            _media.Configure(prefix, manifest);
        }

        public UniTask<string> ResolveVideoUrl(string assetName) =>
            _media.ResolveVideoUrl(assetName);

        public UniTask<string> ResolveAudioUrl(string assetName) =>
            _media.ResolveAudioUrl(assetName);

        public async UniTask<AssetBundle> GetAssetBundle(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) 
            {
                _ctx.OnLog?.Invoke((LogType.Warning, "bundle name is empty"));
                _ctx.OnFailure?.Invoke(new BundleFailure(
                    BundleFailureCodes.InvalidBundleName,
                    "Bundle name is empty."));
                return null;
            }

            return await GetOrLoadBundle(bundleName, true);
        }

        internal async UniTask<AssetBundle> AcquireAssetBundle(string bundleName)
        {
            var bundle = await GetOrLoadBundle(bundleName, false);
            var record = _bundles[GetBundleKey(bundleName)];
            record.Leases++;
            return bundle;
        }

        internal AssetBundle GetOwnedAssetBundle(string bundleName)
        {
            return GetLoadedBundle(bundleName);
        }

        private async UniTask<AssetBundle> GetOrLoadBundle(
            string bundleName,
            bool persistent)
        {
            var bundlesKey = GetBundleKey(bundleName);
            if (!_bundles.TryGetValue(bundlesKey, out var record))
            {
                record = new BundleRecord();
                _bundles.Add(bundlesKey, record);
            }
            record.Persistent |= persistent;
            if (record.Bundle != null)
            {
                _ctx.OnLog?.Invoke((
                    LogType.Log,
                    $"Get bundle {bundleName} from memory"));
                return record.Bundle;
            }
            if (!record.IsLoading)
            {
                record.IsLoading = true;
                record.Loading = LoadBundle(bundleName, bundlesKey, record)
                    .Preserve();
            }
            return await record.Loading;
        }

        private async UniTask<AssetBundle> LoadBundle(
            string bundleName,
            string bundlesKey,
            BundleRecord record)
        {
            var log = (LogType.Warning, "bundle is not loaded");
            try
            {
                var manifest = await GetBundleManifestAsync(bundleName);
                var bundlesVersion = manifest.version;
                var bundlesPath = $"{bundlesKey}/{bundlesVersion}";
                var cachePath = bundlesPath;
                try
                {
                    VerifyFile(
                        bundleName,
                        manifest.size,
                        manifest.sha256,
                        _cache.GetLocalPath(cachePath, false),
                        manifest.HasIntegrity);
                    record.Bundle = await _cache
                        .BundleFromCache(cachePath)
                        .AttachExternalCancellation(_ctx.CancellationToken);
                    if (record.Bundle == null)
                        throw new InvalidDataException(
                            $"Cached bundle '{cachePath}' is invalid.");
                    log = (LogType.Log, $"Get local bundle from {cachePath}");
                }
                catch (OperationCanceledException)
                    when (_ctx.CancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    log = (
                        LogType.Warning,
                        $"No valid local bundle {bundleName}; download "
                        + $"{_source.GetUrl(bundlesPath)}. {exception.Message}");
                    var temporaryPath = _cache.CreateTemporaryPath(cachePath);
                    try
                    {
                        await _source.DownloadFile(bundlesPath, temporaryPath);
                        _ctx.CancellationToken.ThrowIfCancellationRequested();
                        VerifyFile(
                            bundleName,
                            manifest.size,
                            manifest.sha256,
                            temporaryPath,
                            manifest.HasIntegrity);
                        _cache.CommitTemporaryFile(temporaryPath, cachePath);
                    }
                    finally
                    {
                        if (File.Exists(temporaryPath))
                            File.Delete(temporaryPath);
                    }
                    record.Bundle = await _cache
                        .BundleFromCache(cachePath)
                        .AttachExternalCancellation(_ctx.CancellationToken);
                    if (record.Bundle == null)
                        throw new InvalidDataException(
                            $"Downloaded bundle '{bundlesPath}' is invalid.");
                }
                RegisterAssetNames(bundlesKey, record.Bundle);
                _cache.PruneDirectory(bundlesKey, bundlesVersion);
                _cache.TextToCache(GetVersionPointerPath(bundleName), bundlesVersion);
                if (manifest.HasIntegrity)
                {
                    _cache.TextToCache(
                        GetManifestPointerPath(bundleName),
                        JsonUtility.ToJson(manifest));
                }
                _ctx.OnLog?.Invoke(log);
                return record.Bundle;
            }
            finally
            {
                record.IsLoading = false;
            }
        }

        private string GetBundleKey(string bundleName)
        {
            return $"Remote/{GetPlatform()}/{bundleName}";
        }

        internal void ReleaseBundles(IEnumerable<string> bundleNames)
        {
            foreach (var bundleName in bundleNames)
            {
                var bundleKey = GetBundleKey(bundleName);
                if (!_bundles.TryGetValue(bundleKey, out var record))
                    continue;
                record.Leases = Math.Max(0, record.Leases - 1);
                if (record.Leases > 0 || record.Persistent)
                    continue;
                _bundles.Remove(bundleKey);
                record.Bundle?.Unload(false);

                RemoveAssets(_sprites, bundleKey);
                RemoveAssets(_scriptableObjects, bundleKey);
                RemoveAssets(_prefabs, bundleKey);
                _assetNames.Remove(bundleKey);
            }

            _media.Clear();
        }

        private string GetAssetKey(string bundleName, string assetName)
        {
            return $"{GetBundleKey(bundleName)}|{assetName}";
        }

        public string ResolveAssetName(string bundleName, string requestedName)
        {
            if (string.IsNullOrWhiteSpace(requestedName))
                return null;
            var bundleKey = GetBundleKey(bundleName);
            if (_assetNames.TryGetValue(bundleKey, out var names)
                && names.TryGetValue(requestedName, out var actualName))
            {
                return actualName;
            }

            return null;
        }

        private void ReportMissingAsset(string bundleName, string requestedName)
        {
            _ctx.OnFailure?.Invoke(new BundleFailure(
                BundleFailureCodes.AssetNotFound,
                $"Asset '{requestedName}' is absent from bundle '{bundleName}'."));
        }

        private void RegisterAssetNames(string bundleKey, AssetBundle bundle)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assetName in bundle.GetAllAssetNames())
                names[assetName] = assetName;
            _assetNames[bundleKey] = names;
        }

        private static void RemoveAssets<T>(
            IDictionary<string, T> assets,
            string bundleKey)
        {
            var prefix = $"{bundleKey}|";
            var keys = assets.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (var key in keys)
                assets.Remove(key);
        }

        private async UniTask<string> GetBundleVersionAsync(string bundleName)
        {
            var path = $"Remote/{GetPlatform()}/{bundleName}/version.txt";
            try
            {
                var bundlesVersion = (await _source.DownloadText(path)).Trim();
                if (bundlesVersion.Length == 0)
                    throw new InvalidDataException(
                        $"Bundle version is empty for '{bundleName}'.");
                return bundlesVersion;
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                var pointerPath = GetVersionPointerPath(bundleName);
                if (!_cache.Exists(pointerPath))
                {
                    throw new InvalidOperationException(
                        $"Bundle version for '{bundleName}' is unavailable and "
                        + "no last-known-good version is cached.",
                        exception);
                }

                var cachedVersion = _cache.TextFromCache(pointerPath).Trim();
                if (cachedVersion.Length == 0)
                    throw new InvalidDataException(
                        $"Cached bundle version is empty for '{bundleName}'.",
                        exception);

                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"Use cached version '{cachedVersion}' for bundle "
                    + $"'{bundleName}' because the content source is unavailable."));
                return cachedVersion;
            }
        }

        private async UniTask<BundleManifest> GetBundleManifestAsync(
            string bundleName)
        {
            var releaseEntry = _release?.FindBundle(bundleName);
            if (releaseEntry != null)
            {
                var pinned = releaseEntry.ToManifest();
                ValidateManifest(bundleName, pinned);
                return pinned;
            }
            var path = $"Remote/{GetPlatform()}/{bundleName}/manifest.json";
            try
            {
                var manifest = JsonUtility.FromJson<BundleManifest>(
                    await _source.DownloadText(path));
                ValidateManifest(bundleName, manifest);
                return manifest;
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
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
                        _ctx.OnLog?.Invoke((
                            LogType.Warning,
                            $"Use cached integrity manifest for bundle "
                            + $"'{bundleName}' because the content source is unavailable."));
                        return cached;
                    }
                    catch (Exception cachedException)
                    {
                        _ctx.OnLog?.Invoke((
                            LogType.Warning,
                            $"Cached manifest for '{bundleName}' is invalid: "
                            + cachedException.Message));
                    }
                }

                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"Integrity manifest for '{bundleName}' is unavailable; "
                    + $"fall back to legacy version.txt. {exception.Message}"));
                return BundleManifest.Legacy(
                    await GetBundleVersionAsync(bundleName));
            }
        }

        private static void ValidateManifest(
            string bundleName,
            BundleManifest manifest)
        {
            if (manifest == null
                || string.IsNullOrWhiteSpace(manifest.version)
                || manifest.size <= 0
                || string.IsNullOrWhiteSpace(manifest.sha256))
            {
                throw new InvalidDataException(
                    $"Integrity manifest for '{bundleName}' is incomplete.");
            }
        }

        private static void VerifyFile(
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
                .Replace("-", string.Empty)
                .ToLowerInvariant();
            if (!string.Equals(
                    actual,
                    expectedSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ContentIntegrityException(
                    $"Content '{name}' SHA-256 mismatch.");
            }
        }

        private string GetVersionPointerPath(string bundleName)
        {
            return $"Remote/{GetPlatform()}/BundleVersions/{bundleName}.txt";
        }

        private string GetManifestPointerPath(string bundleName)
        {
            return $"Remote/{GetPlatform()}/BundleManifests/{bundleName}.json";
        }

        public async UniTask<ContentRelease> LoadReleaseAsync(
            string clientVersion,
            int supportedSchemaVersion)
        {
            var path = $"Remote/{GetPlatform()}/release.json";
            var cachePath = $"Remote/{GetPlatform()}/Releases/current.json";
            ContentRelease release;
            try
            {
                var json = await _source.DownloadText(path);
                release = JsonUtility.FromJson<ContentRelease>(json);
                ValidateRelease(release, clientVersion, supportedSchemaVersion);
                _cache.TextToCache(cachePath, json);
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ContentCompatibilityException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (!_cache.Exists(cachePath))
                    throw new ContentSourceException(
                        "Content release is unavailable and no cached release exists.",
                        exception);
                release = JsonUtility.FromJson<ContentRelease>(
                    _cache.TextFromCache(cachePath));
                ValidateRelease(release, clientVersion, supportedSchemaVersion);
                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"Use cached content release '{release.releaseId}'."));
            }

            _release = release;
            return release;
        }

        private static void ValidateRelease(
            ContentRelease release,
            string clientVersion,
            int supportedSchemaVersion)
        {
            if (release == null
                || string.IsNullOrWhiteSpace(release.releaseId)
                || release.bundles == null
                || release.bundles.Length == 0)
            {
                throw new ContentIntegrityException(
                    "Content release manifest is incomplete.");
            }
            if (release.contentSchemaVersion > supportedSchemaVersion)
            {
                throw new ContentCompatibilityException(
                    $"Content schema {release.contentSchemaVersion} requires "
                    + $"a newer client (supported: {supportedSchemaVersion}).");
            }
            if (TryParseVersion(release.minimumClientVersion, out var minimum)
                && TryParseVersion(clientVersion, out var current)
                && current < minimum)
            {
                throw new ContentCompatibilityException(
                    $"Content requires client {minimum} or newer; current is {current}.");
            }
            foreach (var bundle in release.bundles)
            {
                if (bundle == null)
                    throw new ContentIntegrityException(
                        "Content release contains an empty bundle entry.");
                ValidateManifest(bundle.name, bundle.ToManifest());
            }
        }

        private static bool TryParseVersion(string value, out Version version)
        {
            if (Version.TryParse(value, out version))
                return true;
            version = null;
            return false;
        }

        private async UniTask<string> ResolveContentFileUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var descriptor = _release?.FindFile(path);
            if (_release != null && descriptor == null)
            {
                throw new ContentIntegrityException(
                    $"File '{path}' is absent from release '{_release.releaseId}'.");
            }
            var releaseId = _release?.releaseId ?? "legacy";
            var cachePath = $"RemoteFiles/{releaseId}/{path}";
            var localPath = _cache.GetLocalPath(cachePath, false);
            var verify = descriptor != null;
            try
            {
                VerifyFile(
                    path,
                    descriptor?.size ?? 0,
                    descriptor?.sha256,
                    localPath,
                    verify);
            }
            catch (Exception)
            {
                var temporaryPath = _cache.CreateTemporaryPath(cachePath);
                try
                {
                    await _source.DownloadFile(path, temporaryPath);
                    _ctx.CancellationToken.ThrowIfCancellationRequested();
                    VerifyFile(
                        path,
                        descriptor?.size ?? 0,
                        descriptor?.sha256,
                        temporaryPath,
                        verify);
                    _cache.CommitTemporaryFile(temporaryPath, cachePath);
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }
            _cache.Touch(cachePath);
            _cache.PruneBySize(
                "RemoteFiles",
                _contentFileCacheLimit,
                cachePath);
            return new Uri(_cache.GetLocalPath(cachePath, false)).AbsoluteUri;
        }

        public async UniTask<string> GetText(string path)
        {
            var url = await ResolveContentFileUrl(path);
            return File.ReadAllText(new Uri(url).LocalPath);
        }

        private string GetPlatform()
        {
#if UNITY_STANDALONE_OSX
            return "Mac";
#elif UNITY_STANDALONE_WIN
            return "Win";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_ANDROID
            return "Android";
#else
            throw new PlatformNotSupportedException(
                "AssetBundle platform is not configured for the active build target.");
#endif
        }

        private AssetBundle GetLoadedBundle(string bundleName)
        {
            var key = GetBundleKey(bundleName);
            if (!_bundles.TryGetValue(key, out var record)
                || record.Bundle == null)
                throw new InvalidOperationException($"AssetBundle '{bundleName}' is not loaded.");
            return record.Bundle;
        }

    }
}
