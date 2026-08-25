using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novels
{
    internal sealed class ContentDeliveryFlow
    {
        private readonly Bundles.Entity _bundles;
        private readonly CancellationToken _cancellationToken;

        internal ContentDeliveryFlow(
            Bundles.Entity bundles,
            CancellationToken cancellationToken)
        {
            _bundles = bundles ?? throw new ArgumentNullException(nameof(bundles));
            _cancellationToken = cancellationToken;
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareStory(
            Bootstrap.BootstrapController bootstrap,
            string contentId)
        {
            return PrepareGroup(
                bootstrap,
                ContentAddressing.ContentPackageConvention.StoryDeliveryGroup(contentId));
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareStoryPreview(
            Bootstrap.BootstrapController bootstrap,
            string contentId)
        {
            var group = ContentAddressing.ContentPackageConvention
                .StoryPreviewDeliveryGroup(contentId);
            return _bundles.HasDeliveryGroup(group)
                ? PrepareGroup(bootstrap, group)
                : PrepareStory(bootstrap, contentId);
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareStoryInBackground(
            string contentId,
            Action<Bundles.ContentDeliveryProgress> onProgress) =>
            _bundles.PrepareDeliveryGroup(
                ContentAddressing.ContentPackageConvention.StoryDeliveryGroup(contentId),
                onProgress,
                _cancellationToken);

        private async UniTask<Bundles.ContentDeliveryLease> PrepareGroup(
            Bootstrap.BootstrapController bootstrap,
            string group)
        {
            if (!_bundles.HasDeliveryGroup(group))
            {
                return null;
            }
            const string message = ApplicationTexts.PreparingContent;
            bootstrap.ShowLoading(message);
            return await _bundles.PrepareDeliveryGroup(
                group,
                progress => bootstrap.ShowLoading(
                    $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                    + $"({progress.Ratio:P0})"),
                _cancellationToken);
        }
    }
}
