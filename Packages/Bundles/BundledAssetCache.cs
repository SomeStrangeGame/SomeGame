using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class BundledAssetCache
    {
        private readonly Dictionary<string, Sprite> _sprites = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ScriptableObject> _scriptableObjects = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameObject> _prefabs = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Dictionary<string, string>> _assetNames = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly CancellationToken _cancellationToken;

        internal BundledAssetCache(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal void Register(string bundleKey, AssetBundle bundle)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assetName in bundle.GetAllAssetNames())
                names[assetName] = assetName;
            _assetNames[bundleKey] = names;
        }

        internal string Resolve(string bundleKey, string requestedName)
        {
            if (string.IsNullOrWhiteSpace(requestedName)
                || !_assetNames.TryGetValue(bundleKey, out var names)
                || !names.TryGetValue(requestedName, out var actualName))
            {
                return null;
            }
            return actualName;
        }

        internal async UniTask<Sprite> GetSprite(
            string bundleName,
            string bundleKey,
            AssetBundle bundle,
            string requestedName)
        {
            var assetName = RequireAsset(bundleName, bundleKey, requestedName);
            return await LoadSprite(bundleKey, bundle, assetName);
        }

        internal async UniTask<Sprite> TryGetSprite(
            string bundleKey,
            AssetBundle bundle,
            string requestedName)
        {
            var assetName = Resolve(bundleKey, requestedName);
            if (assetName == null)
                return null;

            return await LoadSprite(bundleKey, bundle, assetName);
        }

        private async UniTask<Sprite> LoadSprite(
            string bundleKey,
            AssetBundle bundle,
            string assetName)
        {
            var key = GetKey(bundleKey, assetName);
            if (!_sprites.TryGetValue(key, out var sprite))
            {
                sprite = await bundle.LoadAssetAsync<Sprite>(assetName)
                    .WithCancellation(_cancellationToken) as Sprite;
                _sprites[key] = sprite;
            }
            return sprite;
        }

        internal async UniTask<T> GetScriptableObject<T>(
            string bundleName,
            string bundleKey,
            AssetBundle bundle,
            string requestedName)
            where T : ScriptableObject
        {
            var assetName = RequireAsset(bundleName, bundleKey, requestedName);
            var key = GetKey(bundleKey, assetName);
            if (!_scriptableObjects.TryGetValue(key, out var asset))
            {
                asset = await bundle.LoadAssetAsync<T>(assetName)
                    .WithCancellation(_cancellationToken) as T;
                _scriptableObjects[key] = asset;
            }
            return asset as T;
        }

        internal async UniTask<GameObject> GetPrefab(
            string bundleName,
            string bundleKey,
            AssetBundle bundle,
            string requestedName)
        {
            var assetName = RequireAsset(bundleName, bundleKey, requestedName);
            var key = GetKey(bundleKey, assetName);
            if (!_prefabs.TryGetValue(key, out var prefab))
            {
                prefab = await bundle.LoadAssetAsync<GameObject>(assetName)
                    .WithCancellation(_cancellationToken) as GameObject;
                _prefabs[key] = prefab;
            }
            return prefab;
        }

        internal void Remove(string bundleKey)
        {
            Remove(_sprites, bundleKey);
            Remove(_scriptableObjects, bundleKey);
            Remove(_prefabs, bundleKey);
            _assetNames.Remove(bundleKey);
        }

        internal void Clear()
        {
            _sprites.Clear();
            _scriptableObjects.Clear();
            _prefabs.Clear();
            _assetNames.Clear();
        }

        private string RequireAsset(
            string bundleName,
            string bundleKey,
            string requestedName)
        {
            var resolved = Resolve(bundleKey, requestedName);
            if (resolved == null)
            {
                throw new ContentConfigurationException(
                    $"Asset '{requestedName}' is absent from bundle '{bundleName}'.");
            }
            return resolved;
        }

        private static string GetKey(string bundleKey, string assetName) =>
            $"{bundleKey}|{assetName}";

        private static void Remove<T>(IDictionary<string, T> assets, string bundleKey)
        {
            var prefix = $"{bundleKey}|";
            var keys = assets.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            foreach (var key in keys)
                assets.Remove(key);
        }
    }
}
