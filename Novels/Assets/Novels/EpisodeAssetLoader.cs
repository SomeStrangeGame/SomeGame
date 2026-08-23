using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal sealed class EpisodeAssetSet
    {
        internal EpisodeAssetSet(
            GameObject loading,
            GameObject bubble,
            GameObject location,
            GameObject character,
            GameObject notification)
        {
            Loading = loading;
            Bubble = bubble;
            Location = location;
            Character = character;
            Notification = notification;
        }

        internal GameObject Loading { get; }
        internal GameObject Bubble { get; }
        internal GameObject Location { get; }
        internal GameObject Character { get; }
        internal GameObject Notification { get; }
    }

    internal sealed class EpisodeAssetLoader
    {
        internal struct Dependencies
        {
            internal Bundles.Scope Bundles;
            internal PriorityLoader PriorityLoader;
            internal ContentAddressing.ContentAddresses Addresses;
            internal string BundleName;
            internal FallbackAssets Fallbacks;
            internal CancellationToken CancellationToken;
        }

        private readonly Dependencies _ctx;

        internal EpisodeAssetLoader(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.PriorityLoader == null)
                throw new ArgumentNullException(nameof(ctx.PriorityLoader));
            if (ctx.Addresses == null)
                throw new ArgumentNullException(nameof(ctx.Addresses));
            if (string.IsNullOrWhiteSpace(ctx.BundleName))
                throw new ArgumentException("Story bundle name must not be empty.", nameof(ctx.BundleName));
            if (ctx.Fallbacks == null)
                throw new ArgumentNullException(nameof(ctx.Fallbacks));
        }

        internal async UniTask<EpisodeAssetSet> Load()
        {
            var assetName = ContentAddressing.ContentAssetNames.EpisodeScreen;
            var (loading, bubble, location, character, notification) =
                await UniTask.WhenAll(
                    LoadPrefab(
                        _ctx.Addresses.LoadingPrefab(assetName),
                        _ctx.Addresses.SharedLoadingPrefab(assetName),
                        _ctx.Fallbacks.Loading),
                    LoadPrefab(
                        _ctx.Addresses.BubblePrefab(assetName),
                        _ctx.Addresses.SharedBubblePrefab(assetName),
                        _ctx.Fallbacks.Bubble),
                    LoadPrefab(
                        _ctx.Addresses.LocationPrefab(assetName),
                        _ctx.Addresses.SharedLocationPrefab(assetName),
                        _ctx.Fallbacks.Location),
                    LoadPrefab(
                        _ctx.Addresses.CharacterPrefab(assetName),
                        _ctx.Addresses.SharedCharacterPrefab(assetName),
                        _ctx.Fallbacks.CharacterScreen),
                    LoadPrefab(
                        _ctx.Addresses.NotificationPrefab(assetName),
                        _ctx.Addresses.SharedNotificationPrefab(assetName),
                        _ctx.Fallbacks.Notification));
            return new EpisodeAssetSet(
                loading,
                bubble,
                location,
                character,
                notification);
        }

        private async UniTask<GameObject> LoadPrefab(
            string episodeAssetName,
            string sharedAssetName,
            GameObject fallback)
        {
            var episodeAddress = new Bundles.BundleAssetAddress(
                _ctx.BundleName,
                episodeAssetName);
            var prefab = await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .TryGetBundledPrefab(episodeAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            if (prefab != null)
                return prefab;

            var sharedAddress = new Bundles.BundleAssetAddress(
                _ctx.BundleName,
                sharedAssetName);
            var shared = await _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .TryGetBundledPrefab(sharedAddress)
                .AttachExternalCancellation(_ctx.CancellationToken));
            return shared != null ? shared : fallback;
        }
    }
}
