using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Novels.Editor")]
#endif

namespace Novels
{
    // One queue lives across catalog/reading transitions. Bundles and cache leases
    // stay pinned to the preview's release until the application catalog is disposed.
    internal sealed class CatalogDownloads : IDisposable
    {
        private sealed class Story : IDisposable
        {
            internal string Id;
            internal Catalog.Contracts.StoryCatalogPreview Preview;
            internal readonly Catalog.CatalogDownloadState State = new();
            internal Bundles.Entity Bundles;
            internal bool ReleaseLoaded;
            internal IReadOnlyList<Bundles.ContentDeliveryLease> Leases;
            public void Dispose()
            {
                if (Leases != null) foreach (var lease in Leases) lease.Dispose();
                Bundles?.Dispose();
            }
        }

        private readonly CatalogFlow.Dependencies _ctx;
        private readonly Dictionary<string, Story> _stories = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<Story> _queue = new();
        private readonly CancellationTokenSource _lifetime;
        private bool _running;
        private bool _disposed;
        private bool _released;

        internal CatalogDownloads(CatalogFlow.Dependencies ctx,
            IReadOnlyList<Catalog.NovelCatalogEntry> entries,
            IReadOnlyDictionary<string, Catalog.Contracts.StoryCatalogPreview> previews)
        {
            _ctx = ctx;
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken);
            foreach (var entry in entries)
            {
                var story = new Story { Id = entry.ContentId, Preview = previews[entry.ContentId] };
                story.State.RetryRequested += () => Retry(story);
                _stories.Add(story.Id, story);
                _queue.Enqueue(story);
            }
        }

        internal Catalog.CatalogDownloadState GetState(string storyId) => _stories[storyId].State;

        internal Bundles.Entity GetReadyBundles(string storyId)
        {
            var story = _stories[storyId];
            if (!story.State.IsReady) throw new InvalidOperationException("Story content is not downloaded.");
            return story.Bundles;
        }

        internal void Start()
        {
            if (_running || _disposed || _queue.Count == 0) return;
            _running = true;
            Run().Forget(exception => _ctx.OnLog?.Invoke((LogType.Error,
                $"Catalog download queue failed: {exception}")));
        }

        private void Retry(Story story)
        {
            if (_disposed || story.State.Status != Catalog.CatalogDownloadStatus.Failed) return;
            story.State.Update(Catalog.CatalogDownloadStatus.Queued);
            _queue.Enqueue(story);
            Start();
        }

        private async UniTask Run()
        {
            var token = _lifetime.Token;
            try
            {
                // Let the lightweight catalog render before touching any story payload.
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token);
                while (_queue.Count > 0)
                {
                    token.ThrowIfCancellationRequested();
                    var story = _queue.Dequeue();
                    story.State.Update(Catalog.CatalogDownloadStatus.Downloading);
                    _ctx.SmokeTelemetry?.Emit("catalog.download_started", ("contentId", story.Id));
                    try
                    {
                        story.Bundles ??= _ctx.CreateStoryBundles(story.Id, token);
                        if (!story.ReleaseLoaded)
                        {
                            await story.Bundles.LoadReleaseAsync(_ctx.ClientVersion,
                                ContentAddressing.ContentCompatibility.MinimumSupportedSchemaVersion,
                                ContentAddressing.ContentCompatibility.MaximumSupportedSchemaVersion)
                                .AttachExternalCancellation(token);
                            if (story.Bundles.ReleaseId != story.Preview.releaseId)
                                throw new Bundles.ContentConfigurationException(
                                    $"Story '{story.Id}' preview and content release differ; rebuild/publish them together.");
                            story.ReleaseLoaded = true;
                        }
                        story.Leases = await new ContentDeliveryFlow(story.Bundles, token)
                            .PrepareStoryComplete(story.Id, progress =>
                            {
                                if (!_disposed) story.State.Update(Catalog.CatalogDownloadStatus.Downloading, progress);
                            });
                        token.ThrowIfCancellationRequested();
                        story.Bundles.ActivateRelease();
                        story.State.Update(Catalog.CatalogDownloadStatus.Ready, 1f);
                        _ctx.SmokeTelemetry?.Emit("catalog.download_ready", ("contentId", story.Id));
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
                    catch (Exception exception)
                    {
                        if (story.Leases != null) foreach (var lease in story.Leases) lease.Dispose();
                        story.Leases = null;
                        story.State.Update(Catalog.CatalogDownloadStatus.Failed);
                        _ctx.OnLog?.Invoke((LogType.Warning, $"Background download '{story.Id}' failed: {exception}"));
                        _ctx.SmokeTelemetry?.Emit("catalog.download_failed", ("contentId", story.Id));
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally
            {
                _running = false;
                if (_disposed) ReleaseStories();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _lifetime.Cancel();
            _queue.Clear();
            if (!_running) ReleaseStories();
        }

        private void ReleaseStories()
        {
            if (_released) return;
            _released = true;
            foreach (var story in _stories.Values) story.Dispose();
            _stories.Clear();
            _lifetime.Dispose();
        }
    }
}
