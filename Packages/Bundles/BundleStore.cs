using System;
using System.Collections.Generic;
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
        private readonly BundlePayloadLoader _payloads;
        private readonly BundledAssetCache _assets;

        internal BundleStore(
            BundlePayloadLoader payloads,
            CancellationToken cancellationToken)
        {
            _payloads = payloads ?? throw new ArgumentNullException(nameof(payloads));
            _assets = new BundledAssetCache(cancellationToken);
        }

        internal UniTask<AssetBundle> GetPersistent(
            ContentReleaseSession session,
            string bundleName) =>
            GetOrLoad(session, bundleName, true);

        internal async UniTask<AssetBundle> Acquire(
            ContentReleaseSession session,
            string bundleName)
        {
            var bundle = await GetOrLoad(session, bundleName, false);
            _records[GetKey(session, bundleName)].Leases++;
            return bundle;
        }

        internal AssetBundle GetOwned(
            ContentReleaseSession session,
            string bundleName) =>
            GetLoaded(session, bundleName);

        internal UniTask<Sprite> GetSprite(
            ContentReleaseSession session,
            string bundleName,
            string assetName)
        {
            var key = GetKey(session, bundleName);
            return _assets.GetSprite(
                bundleName,
                key,
                GetLoaded(session, bundleName),
                assetName);
        }

        internal UniTask<Sprite> TryGetSprite(
            ContentReleaseSession session,
            string bundleName,
            string assetName)
        {
            var key = GetKey(session, bundleName);
            return _assets.TryGetSprite(
                key,
                GetLoaded(session, bundleName),
                assetName);
        }

        internal UniTask<T> GetScriptableObject<T>(
            ContentReleaseSession session,
            string bundleName,
            string assetName)
            where T : ScriptableObject
        {
            var key = GetKey(session, bundleName);
            return _assets.GetScriptableObject<T>(
                bundleName,
                key,
                GetLoaded(session, bundleName),
                assetName);
        }

        internal UniTask<GameObject> GetPrefab(
            ContentReleaseSession session,
            string bundleName,
            string assetName)
        {
            var key = GetKey(session, bundleName);
            return _assets.GetPrefab(
                bundleName,
                key,
                GetLoaded(session, bundleName),
                assetName);
        }

        internal string ResolveAssetName(
            ContentReleaseSession session,
            string bundleName,
            string requestedName) =>
            _assets.Resolve(GetKey(session, bundleName), requestedName);

        internal void Release(
            ContentReleaseSession session,
            IEnumerable<string> bundleNames)
        {
            foreach (var bundleName in bundleNames)
            {
                var key = GetKey(session, bundleName);
                if (!_records.TryGetValue(key, out var record))
                    continue;
                record.Leases = Math.Max(0, record.Leases - 1);
                if (record.Leases > 0 || record.Persistent)
                    continue;
                Remove(key, record);
            }
        }

        internal void Discard(ContentReleaseSession session)
        {
            if (session == null)
                return;
            foreach (var bundle in session.Release.Bundles)
            {
                var key = GetKey(session, bundle.Name);
                if (!_records.TryGetValue(key, out var record) || record.Leases > 0)
                    continue;
                Remove(key, record);
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
            ContentReleaseSession session,
            string bundleName,
            bool persistent)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(bundleName))
                throw new ContentConfigurationException("Bundle name is empty.");
            var key = GetKey(session, bundleName);
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
                record.Loading = Load(session, bundleName, key, record).Preserve();
            }
            return await record.Loading;
        }

        private async UniTask<AssetBundle> Load(
            ContentReleaseSession session,
            string bundleName,
            string bundleKey,
            Record record)
        {
            try
            {
                record.Bundle = await _payloads.Load(session, bundleName);
                _assets.Register(bundleKey, record.Bundle);
                return record.Bundle;
            }
            finally
            {
                record.IsLoading = false;
            }
        }

        private AssetBundle GetLoaded(
            ContentReleaseSession session,
            string bundleName)
        {
            var key = GetKey(session, bundleName);
            if (!_records.TryGetValue(key, out var record) || record.Bundle == null)
            {
                throw new ContentConfigurationException(
                    $"AssetBundle '{bundleName}' is not loaded for release "
                    + $"'{session.ReleaseId}'.");
            }
            return record.Bundle;
        }

        private void Remove(string key, Record record)
        {
            _records.Remove(key);
            record.Bundle?.Unload(false);
            _assets.Remove(key);
        }

        private static string GetKey(
            ContentReleaseSession session,
            string bundleName)
        {
            var descriptor = session.FindBundle(bundleName)
                ?? throw new ContentIntegrityException(
                    $"Bundle '{bundleName}' is absent from release "
                    + $"'{session.ReleaseId}'.");
            return $"{session.ReleaseId}|{descriptor.Name}|{descriptor.Version}";
        }
    }
}
