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
        private readonly string[] _storyGroups;
        private readonly long _storyBytes;
        private readonly StoryDownloadOverlay _downloadOverlay;
        private readonly StoryStreamingProgressOverlay _progressOverlay;
        private bool _storyDownloadComplete;
        private bool _streamingRunning;

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
            _progressOverlay = StoryStreamingProgressOverlay.Create();
            _assets = (_plan.chunks ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .SelectMany(chunk => (chunk.assets ?? Array.Empty<string>())
                    .Select(asset => (asset, chunk)))
                .ToDictionary(
                    value => Canonicalize(value.asset),
                    value => value.chunk,
                    StringComparer.OrdinalIgnoreCase);
            _readyChunks.Add(0);
            _storyGroups = (_plan.chunks
                    ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .Select(value => value.deliveryGroup)
                .Concat((_plan.media
                        ?? Array.Empty<Bundles.ContentStreamingMediaEntry>())
                    .Select(value => value.deliveryGroup))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            _storyBytes = _storyGroups.Sum(
                _bundles.GetDeliveryGroupSize);
            var initialGroup = (_plan.chunks
                    ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .FirstOrDefault()?.deliveryGroup;
            if (!string.IsNullOrWhiteSpace(initialGroup))
            {
                _downloadedGroupBytes[initialGroup] =
                    _bundles.GetDeliveryGroupSize(initialGroup);
            }
            PublishStoryProgress();
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
                var count = Math.Max(
                    chunks.Length,
                    media.Length == 0 ? 0 : media.Max(value => value.order) + 1);
                for (var index = 0; index < count; index++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    var chunkPreparation = index < chunks.Length
                        ? EnsureChunk(index)
                        : UniTask.CompletedTask;
                    var mediaPreparation = PrepareMediaGroup(media, index);
                    await UniTask.WhenAll(chunkPreparation, mediaPreparation);
                }
                _storyDownloadComplete = true;
                _progressOverlay.Complete();
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _progressOverlay.Interrupted(CalculateStoryProgress());
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
            if (_streamingRunning || _storyDownloadComplete)
                return;
            _streamingRunning = true;
            Run().Forget();
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
            ReportStoryProgress(progress);
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
                ReportStoryProgress,
                _cancellationToken);
            lease.AddTo(this);
        }

        private UniTask PrepareMediaGroup(
            IReadOnlyCollection<Bundles.ContentStreamingMediaEntry> media,
            int order)
        {
            var groups = media
                .Where(value => value.order == order)
                .Select(value => value.deliveryGroup)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(group => PrepareMedia(new Bundles.ContentStreamingMediaEntry
                {
                    order = order,
                    deliveryGroup = group,
                }))
                .ToArray();
            return groups.Length == 0 ? UniTask.CompletedTask : UniTask.WhenAll(groups);
        }

        private void ReportStoryProgress(
            Bundles.ContentDeliveryProgress progress)
        {
            _downloadedGroupBytes[progress.GroupId] = Math.Min(
                progress.CompletedBytes,
                progress.TotalBytes);
            PublishStoryProgress();
        }

        private void PublishStoryProgress()
        {
            _progressOverlay.Report(CalculateStoryProgress());
        }

        private float CalculateStoryProgress()
        {
            var completed = _storyGroups.Sum(group =>
                _downloadedGroupBytes.TryGetValue(group, out var bytes)
                    ? bytes
                    : 0L);
            return _storyBytes <= 0L
                ? 0f
                : Mathf.Clamp01((float)completed / _storyBytes);
        }

        private static string Canonicalize(string value) =>
            (value ?? string.Empty)
                .Replace('\\', '/')
                .Normalize(NormalizationForm.FormC)
                .Trim()
                .ToLowerInvariant();

        protected override void OnDispose()
        {
            if (_downloadOverlay != null)
                UnityEngine.Object.Destroy(_downloadOverlay.gameObject);
            if (_progressOverlay != null)
                UnityEngine.Object.Destroy(_progressOverlay.gameObject);
            base.OnDispose();
        }
    }
}
