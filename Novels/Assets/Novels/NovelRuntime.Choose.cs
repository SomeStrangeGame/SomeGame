using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private static Choose.ChooseController CreateChoose(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            var choose = new Choose.ChooseController(new Choose.ChooseController.Dependencies
            {
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            choose.Init();
            return choose;
        }

        private async UniTask<Sprite> GetChooseSprite(
            PreparedNovelResources state,
            string assetName)
        {
            var cancellationToken = state.CancellationToken;
            var episodeSprite = await _priorityLoader.Run(() => state.EpisodeBundles
                .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                    _episode.BundleName,
                    state.Addresses.ChooseItem(assetName)))
                .AttachExternalCancellation(cancellationToken));
            if (episodeSprite != null)
                return episodeSprite;

            var sharedSprite = await _priorityLoader.Run(() => state.NovelBundles
                .TryGetBundledSprite(new Bundles.BundleAssetAddress(
                    _definition.BundleName,
                    state.Addresses.SharedChooseItem(assetName)))
                .AttachExternalCancellation(cancellationToken));
            return sharedSprite != null ? sharedSprite : _ctx.FallbackAssets.Background;
        }
    }
}
