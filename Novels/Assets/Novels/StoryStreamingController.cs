using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal sealed class StoryStreamingController : BaseDisposable
    {
        private readonly Bundles.Entity _bundles;
        private readonly Bundles.Scope _scope;
        private readonly Bundles.ContentStreamingPlanEntry _plan;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<(LogType type, string message)> _onLog;
        private readonly Action<int> _onChunkReady;
        private readonly Dictionary<string, Bundles.ContentStreamingChunkEntry> _assets;
        private readonly Dictionary<int, UniTaskCompletionSource> _chunkTasks = new();
        private readonly Dictionary<int, Bundles.ContentDeliveryProgress> _chunkProgress = new();
        private readonly HashSet<int> _blockingChunks = new();
        private readonly HashSet<int> _readyChunks = new();
        private readonly Dictionary<string, long> _downloadedGroupBytes = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly string[] _downloadAllGroups;
        private readonly long _downloadAllBytes;
        private readonly StoryDownloadOverlay _downloadOverlay;
        private bool _downloadAllRequested;
        private bool _downloadAllComplete;
        private bool _streamingRunning;

        internal Catalog.CatalogAction DownloadAllAction { get; }

        internal StoryStreamingController(
            Bundles.Entity bundles,
            Bundles.Scope scope,
            Bundles.ContentStreamingPlanEntry plan,
            CancellationToken cancellationToken,
            Action<(LogType type, string message)> onLog,
            Action<int> onChunkReady)
        {
            _bundles = bundles ?? throw new ArgumentNullException(nameof(bundles));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));
            _cancellationToken = cancellationToken;
            _onLog = onLog;
            _onChunkReady = onChunkReady;
            _downloadOverlay = StoryDownloadOverlay.Create();
            DownloadAllAction = new Catalog.CatalogAction(
                "Скачать всю историю",
                RequestDownloadAll);
            _downloadOverlay.BindDownloadAll(DownloadAllAction);
            _assets = (_plan.chunks ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .SelectMany(chunk => (chunk.assets ?? Array.Empty<string>())
                    .Select(asset => (asset, chunk)))
                .ToDictionary(
                    value => Canonicalize(value.asset),
                    value => value.chunk,
                    StringComparer.OrdinalIgnoreCase);
            _readyChunks.Add(0);
            _downloadAllGroups = (_plan.chunks
                    ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .Select(value => value.deliveryGroup)
                .Concat((_plan.media
                        ?? Array.Empty<Bundles.ContentStreamingMediaEntry>())
                    .Select(value => value.deliveryGroup))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _downloadAllBytes = _downloadAllGroups.Sum(
                _bundles.GetDeliveryGroupSize);
            var initialGroup = (_plan.chunks
                    ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .FirstOrDefault()?.deliveryGroup;
            if (!string.IsNullOrWhiteSpace(initialGroup))
            {
                _downloadedGroupBytes[initialGroup] =
                    _bundles.GetDeliveryGroupSize(initialGroup);
            }
            PublishDownloadAllState();
        }

        internal void Start() => StartStreaming();

        internal UniTask<Sprite> GetSprite(string assetName) =>
            LoadSprite(assetName);

        internal UniTask<Sprite> GetFullSprite(string assetName) =>
            LoadSprite(assetName);

        private async UniTask<Sprite> LoadSprite(string assetName)
        {
            if (!_assets.TryGetValue(Canonicalize(assetName), out var chunk))
            {
                var initial = (_plan.chunks
                        ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                    .FirstOrDefault();
                if (initial == null)
                    return null;
                return await _scope.TryGetBundledSprite(
                    new Bundles.BundleAssetAddress(initial.bundle, assetName));
            }
            await EnsureChunkForDemand(chunk.index);
            return await _scope.TryGetBundledSprite(
                new Bundles.BundleAssetAddress(chunk.bundle, assetName));
        }

        private async UniTask EnsureChunkForDemand(int index)
        {
            if (_readyChunks.Contains(index))
                return;
            var preparation = EnsureChunk(index);
            var delay = UniTask.Delay(
                TimeSpan.FromSeconds(0.7d),
                DelayType.UnscaledDeltaTime,
                PlayerLoopTiming.Update,
                _cancellationToken);
            if (await UniTask.WhenAny(preparation, delay) == 0)
                return;
            BeginBlockingWait(index);
            try
            {
                await EnsureChunk(index);
            }
            finally
            {
                EndBlockingWait(index);
            }
        }

        private async UniTaskVoid Run()
        {
            try
            {
                var chunks = _plan.chunks ?? Array.Empty<Bundles.ContentStreamingChunkEntry>();
                var media = _plan.media ?? Array.Empty<Bundles.ContentStreamingMediaEntry>();
                var count = Math.Max(chunks.Length, media.Length);
                for (var index = 0; index < count; index++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    StreamingExperimentDiagnostics.SetQueue(
                        QueueLabel(chunks, media, index));
                    var chunkPreparation = index < chunks.Length
                        ? EnsureChunk(index)
                        : UniTask.CompletedTask;
                    var mediaPreparation = index < media.Length
                        ? PrepareMedia(media[index])
                        : UniTask.CompletedTask;
                    await UniTask.WhenAll(chunkPreparation, mediaPreparation);
                }
                StreamingExperimentDiagnostics.SetQueue("complete");
                StreamingExperimentDiagnostics.SetQuality("Full");
                _downloadAllComplete = true;
                PublishDownloadAllState();
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _downloadAllRequested = false;
                DownloadAllAction.SetState("Продолжить загрузку", true);
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Predictive story streaming stopped: {exception.Message}"));
            }
            finally
            {
                _streamingRunning = false;
            }
        }

        private void StartStreaming()
        {
            if (_streamingRunning || _downloadAllComplete)
                return;
            _streamingRunning = true;
            Run().Forget();
        }

        private void RequestDownloadAll()
        {
            if (_downloadAllComplete)
                return;
            _downloadAllRequested = true;
            PublishDownloadAllState();
            StartStreaming();
        }

        private UniTask EnsureChunk(int index)
        {
            if (_readyChunks.Contains(index))
                return UniTask.CompletedTask;
            if (!_chunkTasks.TryGetValue(index, out var completion))
            {
                completion = new UniTaskCompletionSource();
                _chunkTasks.Add(index, completion);
                PrepareChunk(index, completion).Forget();
            }
            return completion.Task;
        }

        private async UniTaskVoid PrepareChunk(
            int index,
            UniTaskCompletionSource completion)
        {
            try
            {
                var chunk = (_plan.chunks
                        ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                    .FirstOrDefault(value => value.index == index)
                    ?? throw new Bundles.ContentConfigurationException(
                        $"Streaming chunk {index} is absent.");
                var lease = await _bundles.PrepareDeliveryGroup(
                    chunk.deliveryGroup,
                    progress => ReportChunkProgress(index, progress),
                    _cancellationToken);
                lease.AddTo(this);
                await _scope.GetAssetBundle(chunk.bundle);
                _readyChunks.Add(index);
                _onChunkReady?.Invoke(index);
                completion.TrySetResult();
            }
            catch (OperationCanceledException)
                when (_cancellationToken.IsCancellationRequested)
            {
                completion.TrySetCanceled(_cancellationToken);
            }
            catch (Exception exception)
            {
                _chunkTasks.Remove(index);
                completion.TrySetException(exception);
            }
        }

        private void ReportChunkProgress(
            int index,
            Bundles.ContentDeliveryProgress progress)
        {
            _chunkProgress[index] = progress;
            ReportDownloadAllProgress(progress);
            StreamingExperimentDiagnostics.ReportDelivery(progress);
            if (_blockingChunks.Contains(index))
                _downloadOverlay.Report(progress);
        }

        private void BeginBlockingWait(int index)
        {
            _blockingChunks.Add(index);
            RefreshBlockingOverlay();
        }

        private void EndBlockingWait(int index)
        {
            _blockingChunks.Remove(index);
            RefreshBlockingOverlay();
        }

        private void RefreshBlockingOverlay()
        {
            if (_blockingChunks.Count == 0)
            {
                _downloadOverlay.Hide();
                return;
            }
            var index = _blockingChunks.Min();
            var chunk = (_plan.chunks
                    ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .First(value => value.index == index);
            _downloadOverlay.Show(
                chunk.deliveryGroup,
                _chunkProgress.TryGetValue(index, out var progress)
                    ? progress
                    : null);
        }

        private async UniTask PrepareMedia(Bundles.ContentStreamingMediaEntry media)
        {
            var lease = await _bundles.PrepareDeliveryGroup(
                media.deliveryGroup,
                progress =>
                {
                    ReportDownloadAllProgress(progress);
                    StreamingExperimentDiagnostics.ReportDelivery(progress);
                },
                _cancellationToken);
            lease.AddTo(this);
        }

        private void ReportDownloadAllProgress(
            Bundles.ContentDeliveryProgress progress)
        {
            _downloadedGroupBytes[progress.GroupId] = Math.Min(
                progress.CompletedBytes,
                progress.TotalBytes);
            PublishDownloadAllState();
        }

        private void PublishDownloadAllState()
        {
            if (_downloadAllComplete)
            {
                DownloadAllAction.SetState("История загружена", false);
                return;
            }
            if (!_downloadAllRequested)
            {
                DownloadAllAction.SetState("Скачать всю историю", true);
                return;
            }
            var completed = _downloadAllGroups.Sum(group =>
                _downloadedGroupBytes.TryGetValue(group, out var bytes)
                    ? bytes
                    : 0L);
            var ratio = _downloadAllBytes <= 0L
                ? 0f
                : Mathf.Clamp01((float)completed / _downloadAllBytes);
            DownloadAllAction.SetState(
                $"Загрузка всей истории · {ratio:P0}",
                false);
        }

        private static string Canonicalize(string value) =>
            (value ?? string.Empty)
                .Replace('\\', '/')
                .Normalize(NormalizationForm.FormC)
                .Trim()
                .ToLowerInvariant();

        private static string QueueLabel(
            IReadOnlyList<Bundles.ContentStreamingChunkEntry> chunks,
            IReadOnlyList<Bundles.ContentStreamingMediaEntry> media,
            int index)
        {
            var values = new List<string>(4);
            for (var offset = 0; offset < 2; offset++)
            {
                var current = index + offset;
                if (current < chunks.Count)
                    values.Add($"chunk-{current}");
                if (current < media.Count)
                    values.Add($"media-{current}");
            }
            return string.Join(" → ", values);
        }

        protected override void OnDispose()
        {
            if (_downloadOverlay != null)
                UnityEngine.Object.Destroy(_downloadOverlay.gameObject);
            base.OnDispose();
        }
    }
}
