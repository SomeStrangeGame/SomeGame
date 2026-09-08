using System;
using System.Collections.Generic;
using System.Linq;
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
            var group = ContentAddressing.ContentPackageConvention
                .StoryDeliveryGroup(contentId);
            return PrepareGroup(bootstrap, group);
        }

        internal UniTask<Bundles.ContentDeliveryLease> PrepareStoryInitial(
            Bootstrap.BootstrapController bootstrap,
            string contentId)
        {
            var chunks = _bundles.StreamingPlan?.chunks;
            return chunks != null && chunks.Length > 0
                ? PrepareGroup(bootstrap, chunks[0].deliveryGroup)
                : PrepareStory(bootstrap, contentId);
        }

        internal async UniTask<IReadOnlyList<Bundles.ContentDeliveryLease>>
            PrepareStoryComplete(
                Bootstrap.BootstrapController bootstrap,
                string contentId)
            => await PrepareStoryComplete(contentId, progress =>
                bootstrap.ShowLoading($"{ApplicationTexts.PreparingContent} {progress:P0}"));

        internal async UniTask<IReadOnlyList<Bundles.ContentDeliveryLease>> PrepareStoryComplete(
            string contentId,
            Action<float> onProgress)
        {
            var groups = new List<string>
            {
                ContentAddressing.ContentPackageConvention.StoryDeliveryGroup(contentId),
                ContentAddressing.ContentPackageConvention.StoryMediaDeliveryGroup(contentId),
            };
            var streaming = _bundles.StreamingPlan;
            if (streaming?.chunks != null)
                groups.AddRange(streaming.chunks.Select(chunk => chunk.deliveryGroup));
            if (streaming?.media != null)
                groups.AddRange(streaming.media.Select(media => media.deliveryGroup));

            var leases = new List<Bundles.ContentDeliveryLease>();
            var orderedGroups = groups.Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase).Where(_bundles.HasDeliveryGroup).ToArray();
            if (orderedGroups.Length == 0)
                throw new Bundles.ContentConfigurationException($"Story '{contentId}' has no delivery groups.");
            var totalBytes = orderedGroups.Sum(_bundles.GetDeliveryGroupSize);
            long completedBytes = 0;
            try
            {
                foreach (var group in orderedGroups)
                {
                    var size = _bundles.GetDeliveryGroupSize(group);
                    var lease = await _bundles.PrepareDeliveryGroup(group,
                        progress => onProgress?.Invoke(totalBytes > 0
                            ? (float)((completedBytes + size * (double)progress.Ratio) / totalBytes)
                            : 0f), _cancellationToken);
                    if (lease != null)
                        leases.Add(lease);
                    completedBytes += size;
                }
                onProgress?.Invoke(1f);
                return leases;
            }
            catch
            {
                foreach (var lease in leases)
                    lease.Dispose();
                throw;
            }
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
                progress =>
                {
                    bootstrap.ShowLoading(
                        $"{message} {progress.CompletedItems}/{progress.TotalItems} "
                        + $"({progress.Ratio:P0})");
                },
                _cancellationToken);
        }
    }
}
