using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    internal sealed class ContentDeliveryCoordinator
    {
        private readonly ContentReleaseProvider _releases;
        private readonly ContentFileStore _files;

        internal ContentDeliveryCoordinator(
            ContentReleaseProvider releases,
            ContentFileStore files)
        {
            _releases = releases ?? throw new ArgumentNullException(nameof(releases));
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        internal async UniTask Prepare(
            string groupId,
            Action<ContentDeliveryProgress> onProgress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("Delivery group ID must not be empty.", nameof(groupId));
            var release = _releases.Current ?? throw new ContentConfigurationException(
                "Content release must be loaded before delivery preparation.");
            var group = release.DeliveryGroups.FirstOrDefault(value => string.Equals(
                value.Id,
                groupId,
                StringComparison.OrdinalIgnoreCase)) ?? throw new ContentConfigurationException(
                $"Delivery group '{groupId}' is absent from release '{release.ReleaseId}'.");
            var files = release.Files
                .Where(value => string.Equals(
                    value.DeliveryGroup,
                    group.Id,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            _files.ReserveGroup(files);
            var completedBytes = 0L;
            onProgress?.Invoke(new ContentDeliveryProgress(
                group.Id,
                0,
                files.Length,
                0,
                group.Size));
            try
            {
                for (var index = 0; index < files.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _files.ResolveUrl(files[index].Path)
                        .AttachExternalCancellation(cancellationToken);
                    completedBytes += files[index].Size;
                    onProgress?.Invoke(new ContentDeliveryProgress(
                        group.Id,
                        index + 1,
                        files.Length,
                        completedBytes,
                        group.Size));
                }
            }
            catch
            {
                _files.ReleaseGroupReservation(files);
                throw;
            }
        }
    }
}
