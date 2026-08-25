using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<int> _readyChunks = new();

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
            _assets = (_plan.chunks ?? Array.Empty<Bundles.ContentStreamingChunkEntry>())
                .SelectMany(chunk => (chunk.assets ?? Array.Empty<string>())
                    .Select(asset => (asset, chunk)))
                .ToDictionary(
                    value => Canonicalize(value.asset),
                    value => value.chunk,
                    StringComparer.OrdinalIgnoreCase);
        }

        internal void Start() => Run().Forget();

        internal UniTask<Sprite> GetSprite(string assetName) =>
            LoadSprite(assetName, false);

        internal UniTask<Sprite> GetFullSprite(string assetName) =>
            LoadSprite(assetName, true);

        private async UniTask<Sprite> LoadSprite(string assetName, bool requireFull)
        {
            if (!_assets.TryGetValue(Canonicalize(assetName), out var chunk))
            {
                return await _scope.TryGetBundledSprite(
                    new Bundles.BundleAssetAddress(_plan.previewBundle, assetName));
            }
            var bundle = _plan.previewBundle;
            if (requireFull || chunk.index > 0 || _readyChunks.Contains(chunk.index))
            {
                await EnsureChunk(chunk.index);
                bundle = chunk.bundle;
            }
            return await _scope.TryGetBundledSprite(
                new Bundles.BundleAssetAddress(bundle, assetName));
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
                    if (index < chunks.Length)
                        await EnsureChunk(index);
                    if (index < media.Length)
                        await PrepareMedia(media[index]);
                }
                StreamingExperimentDiagnostics.SetQueue("complete");
                StreamingExperimentDiagnostics.SetQuality("Full");
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _onLog?.Invoke((
                    LogType.Warning,
                    $"Predictive story streaming stopped: {exception.Message}"));
            }
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
                    StreamingExperimentDiagnostics.ReportDelivery,
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
                completion.TrySetException(exception);
            }
        }

        private async UniTask PrepareMedia(Bundles.ContentStreamingMediaEntry media)
        {
            var lease = await _bundles.PrepareDeliveryGroup(
                media.deliveryGroup,
                StreamingExperimentDiagnostics.ReportDelivery,
                _cancellationToken);
            lease.AddTo(this);
        }

        private static string Canonicalize(string value) =>
            (value ?? string.Empty).Replace('\\', '/').Trim().ToLowerInvariant();

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
    }
}
