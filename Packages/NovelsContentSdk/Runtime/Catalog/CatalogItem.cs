using System;
using System.Collections.Generic;
using System.Linq;

namespace Novels.Catalog
{
    public enum CatalogDownloadStatus { Queued, Downloading, Ready, Failed }

    public sealed class CatalogDownloadState
    {
        public CatalogDownloadStatus Status { get; private set; } = CatalogDownloadStatus.Queued;
        public float Progress { get; private set; }
        public bool IsReady => Status == CatalogDownloadStatus.Ready;
        public event Action Changed;
        public event Action RetryRequested;

        public void Update(CatalogDownloadStatus status, float progress = 0f)
        {
            progress = UnityEngine.Mathf.Clamp01(progress);
            if (Status == status && Math.Abs(Progress - progress) < .001f) return;
            Status = status;
            Progress = progress;
            Changed?.Invoke();
        }

        public void Retry() { if (Status == CatalogDownloadStatus.Failed) RetryRequested?.Invoke(); }
    }

    public sealed class CatalogEpisodeItem
    {
        public CatalogEpisodeItem(
            string id,
            string title,
            string description = null,
            string status = null,
            string actionLabel = null,
            string restartLabel = null,
            string restartWarning = null,
            bool isEnabled = true,
            CatalogDownloadState download = null,
            UnityEngine.Sprite cover = null,
            string author = null,
            string storyAuthor = null,
            string videoUrl = null,
            float? readingProgress = null)
        {
            Id = string.IsNullOrWhiteSpace(id)
                ? throw new ArgumentException("Episode id must not be empty.", nameof(id))
                : id;
            Title = string.IsNullOrWhiteSpace(title)
                ? throw new ArgumentException("Episode title must not be empty.", nameof(title))
                : title;
            Description = description?.Trim() ?? string.Empty;
            Status = status ?? string.Empty;
            ActionLabel = actionLabel ?? string.Empty;
            RestartLabel = restartLabel ?? string.Empty;
            RestartWarning = restartWarning ?? string.Empty;
            IsEnabled = isEnabled;
            Download = download;
            Cover = cover;
            Author = (string.IsNullOrWhiteSpace(author) ? storyAuthor : author)?.Trim() ?? string.Empty;
            VideoUrl = videoUrl?.Trim() ?? string.Empty;
            ReadingProgress = readingProgress.HasValue && !float.IsNaN(readingProgress.Value)
                && !float.IsInfinity(readingProgress.Value)
                ? UnityEngine.Mathf.Clamp01(readingProgress.Value) : null;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Status { get; }
        public string ActionLabel { get; }
        public string RestartLabel { get; }
        public string RestartWarning { get; }
        public bool IsEnabled { get; }
        public CatalogDownloadState Download { get; }
        public UnityEngine.Sprite Cover { get; }
        public string Author { get; }
        public string VideoUrl { get; }
        public float? ReadingProgress { get; }
    }

    public sealed class CatalogItem
    {
        public CatalogItem(
            string id,
            string title,
            string genre = null,
            string description = null,
            string status = null,
            string actionLabel = null,
            string secondaryActionLabel = null,
            bool isEnabled = true,
            UnityEngine.Sprite cover = null,
            IEnumerable<CatalogEpisodeItem> episodes = null)
        {
            Id = id;
            Title = title;
            Genre = genre;
            Description = description;
            Status = status;
            ActionLabel = actionLabel;
            SecondaryActionLabel = secondaryActionLabel;
            IsEnabled = isEnabled;
            Cover = cover;
            Episodes = Array.AsReadOnly(
                (episodes ?? Array.Empty<CatalogEpisodeItem>())
                    .Where(episode => episode != null)
                    .ToArray());
        }

        public string Id { get; }
        public string Title { get; }
        public string Genre { get; }
        public string Description { get; }
        public string Status { get; }
        public string ActionLabel { get; }
        public string SecondaryActionLabel { get; }
        public bool IsEnabled { get; }
        public UnityEngine.Sprite Cover { get; }
        public IReadOnlyList<CatalogEpisodeItem> Episodes { get; }
    }
}
