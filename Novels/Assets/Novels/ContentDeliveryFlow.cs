using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Novels
{
    internal sealed class ContentDeliveryFlow
    {
        private readonly Bundles.Entity _bundles;
        private readonly ApplicationLocalization _localization;
        private readonly CancellationToken _cancellationToken;

        internal ContentDeliveryFlow(
            Bundles.Entity bundles,
            ApplicationLocalization localization,
            CancellationToken cancellationToken)
        {
            _bundles = bundles ?? throw new ArgumentNullException(nameof(bundles));
            _localization = localization
                ?? throw new ArgumentNullException(nameof(localization));
            _cancellationToken = cancellationToken;
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareNovel(
            Bootstrap.Entity bootstrap,
            string contentId)
        {
            return PrepareGroup(
                bootstrap,
                ContentAddressing.ContentPackageConvention.SharedDeliveryGroup(contentId));
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareEpisode(
            Bootstrap.Entity bootstrap,
            Content.NovelDefinition definition,
            Content.EpisodeDefinition episode)
        {
            return PrepareGroup(
                bootstrap,
                ContentAddressing.ContentPackageConvention.EpisodeDeliveryGroup(
                    definition.Id,
                    episode.Id));
        }

        private async UniTask<Bundles.ContentDeliveryLease> PrepareGroup(
            Bootstrap.Entity bootstrap,
            string group)
        {
            if (_bundles.DeliveryMode == Bundles.ContentDeliveryMode.Embedded
                || !_bundles.HasDeliveryGroup(group))
            {
                return null;
            }
            var message = _localization.Get(ApplicationText.PreparingContent);
            bootstrap.ShowLoading(message);
            try
            {
                return await _bundles.PrepareDeliveryGroup(
                    group,
                    progress => bootstrap.ShowLoading(
                        $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                        + $"({progress.Ratio:P0})"),
                    _cancellationToken);
            }
            finally
            {
                bootstrap.Hide();
            }
        }
    }
}
