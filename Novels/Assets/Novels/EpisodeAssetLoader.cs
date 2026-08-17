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
        internal struct Ctx
        {
            internal Bundles.Scope Bundles;
            internal PriorityLoader PriorityLoader;
            internal ContentAddressing.ContentAddresses Addresses;
            internal string BundleName;
            internal CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;

        internal EpisodeAssetLoader(Ctx ctx)
        {
            _ctx = ctx;
            if (ctx.Bundles == null)
                throw new ArgumentNullException(nameof(ctx.Bundles));
            if (ctx.PriorityLoader == null)
                throw new ArgumentNullException(nameof(ctx.PriorityLoader));
            if (ctx.Addresses == null)
                throw new ArgumentNullException(nameof(ctx.Addresses));
            if (string.IsNullOrWhiteSpace(ctx.BundleName))
                throw new ArgumentException("Episode bundle name must not be empty.", nameof(ctx.BundleName));
        }

        internal async UniTask<EpisodeAssetSet> Load()
        {
            var assetName = ContentAddressing.ContentAssetNames.Screen;
            var (loading, bubble, location, character, notification) =
                await UniTask.WhenAll(
                LoadPrefab(_ctx.Addresses.LoadingPrefab(assetName)),
                LoadPrefab(_ctx.Addresses.BubblePrefab(assetName)),
                LoadPrefab(_ctx.Addresses.LocationPrefab(assetName)),
                LoadPrefab(_ctx.Addresses.CharacterPrefab(assetName)),
                LoadPrefab(_ctx.Addresses.NotificationPrefab(assetName)));
            return new EpisodeAssetSet(
                loading,
                bubble,
                location,
                character,
                notification);
        }

        private UniTask<GameObject> LoadPrefab(string assetName)
        {
            var address = new Bundles.BundleAssetAddress(_ctx.BundleName, assetName);
            return _ctx.PriorityLoader.Run(() => _ctx.Bundles
                .GetBundledPrefab(address)
                .AttachExternalCancellation(_ctx.CancellationToken));
        }
    }
}
