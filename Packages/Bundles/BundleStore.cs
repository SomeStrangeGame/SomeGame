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
        private readonly string _platform;

        internal BundleStore(
            BundlePayloadLoader payloads,
            string platform,
            CancellationToken cancellationToken)
        {
            _payloads = payloads ?? throw new ArgumentNullException(nameof(payloads));
            _platform = platform;
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
                record.Bundle = await _payloads.Load(bundleName, bundleKey);
                _assets.Register(bundleKey, record.Bundle);
                return record.Bundle;
            }
            finally
            {
                record.IsLoading = false;
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

    }
}
