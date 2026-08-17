using System;
using System.Linq;
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

        internal async UniTask PrepareEpisode(
            Bootstrap.Entity bootstrap,
            Content.NovelDefinition definition,
            Content.EpisodeDefinition episode)
        {
            if (_bundles.DeliveryMode == Bundles.ContentDeliveryMode.Embedded)
                return;
            var message = _localization.Get(ApplicationText.PreparingContent);
            bootstrap.ShowLoading(message);
            try
            {
                var groups = new[]
                {
                    ContentAddressing.ContentPackageConvention.SharedDeliveryGroup(
                        definition.Id),
                    ContentAddressing.ContentPackageConvention.EpisodeDeliveryGroup(
                        definition.Id,
                        episode.Id),
                };
                foreach (var group in groups.Where(_bundles.HasDeliveryGroup))
                {
                    await _bundles.PrepareDeliveryGroup(
                        group,
                        progress => bootstrap.ShowLoading(
                            $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                            + $"({progress.Ratio:P0})"),
                        _cancellationToken);
                }
            }
            finally
            {
                bootstrap.Hide();
            }
        }
    }
}
