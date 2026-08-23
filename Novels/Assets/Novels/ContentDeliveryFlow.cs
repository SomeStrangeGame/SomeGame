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
