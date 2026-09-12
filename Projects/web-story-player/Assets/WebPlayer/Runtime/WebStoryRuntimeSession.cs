using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.WebPlayer
{
    /// <summary>
    /// Owns one accepted browser launch and the shared episode-runtime lifetime.
    /// Normal shutdown drains reading and durable saves before releasing resources.
    /// </summary>
    public sealed class WebStoryRuntimeSession : IDisposable
    {
        private readonly CancellationTokenSource _cancellation = new();
        private readonly GameObject _runtimeRoot;
        private bool _disposed;
        private bool _started;
        private readonly UniTaskCompletionSource _finished = new();
        private WebEpisodePlayer _player;

        public async UniTask<EpisodeRunResult> Run(WebStorySaveStore store,
            string pageUrl, Action<string, string> emit)
        {
            if (_started || _disposed) throw new InvalidOperationException("Session cannot be restarted.");
            _started = true;
            try
            {
                await PrepareContent(pageUrl);
                emit("content_ready", "");
                return await Read(store, emit);
            }
            finally { _finished.TrySetResult(); }
        }

        public async UniTask StopAsync()
        {
            if (_disposed) return;
            _player?.StopInput();
            _cancellation.Cancel();
            if (_started) await _finished.Task;
            // Retry a failed transaction using the latest in-memory snapshot.
            // On failure keep the cancelled session alive for a subsequent retry.
            if (!_disposed && _player != null) await _player.FlushForStop();
            Dispose();
        }

        private WebStoryRuntimeSession(WebPlayerLaunchConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(
                nameof(configuration));
            _runtimeRoot = new GameObject($"StoryRuntime[{configuration.storyId}]");
            UnityEngine.Object.DontDestroyOnLoad(_runtimeRoot);
            Episode = new EpisodeRuntime(_cancellation.Token);
        }

        public WebPlayerLaunchConfiguration Configuration { get; }
        public EpisodeRuntime Episode { get; }
        public Transform RuntimeRoot => _runtimeRoot.transform;
        public CancellationToken CancellationToken => _cancellation.Token;
        public Bundles.Entity Content { get; private set; }
        public Bundles.Scope Assets { get; private set; }
        public string EpisodeId => _player?.EpisodeId ?? "";
        public bool HasNextEpisode => _player?.HasNextEpisode == true;

        public UniTask<EpisodeRunResult> Read(WebStorySaveStore store, Action<string, string> emit)
        {
            CancellationToken.ThrowIfCancellationRequested();
            _player = new WebEpisodePlayer(this, emit).AddTo(Episode.Scope);
            return _player.Run(store);
        }

        public async UniTask PrepareContent(string pageUrl)
        {
            if (Content != null) throw new InvalidOperationException("Content is already initialized.");
            Content = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                ContentSource = new WebStoryContentSource(pageUrl, Configuration, CancellationToken),
                PersistentDataPath = Application.temporaryCachePath,
                CacheNamespace = Configuration.storyId + "/" + Configuration.storyVersion,
                Platform = "WebGL",
                DeliveryOptions = new Bundles.ContentDeliveryOptions(
                    64L * 1024 * 1024, 1, TimeSpan.FromDays(1),
                    Bundles.ContentRequestPolicy.RemoteDefault, Bundles.ContentRequestPolicy.LocalDefault),
                CancellationToken = CancellationToken,
                OnLog = message => Debug.Log(message.message),
            }).AddTo(Episode.Scope);
            await Content.LoadReleaseAsync("0.2.0", 5, 5);
            CancellationToken.ThrowIfCancellationRequested();
            Content.ActivateRelease();
            var chunks = Content.StreamingPlan?.chunks;
            if (chunks == null || chunks.Length == 0)
                throw new InvalidOperationException("Expected a streaming story release.");
            Assets = Content.CreateScope().AddTo(Episode.Scope);
            await Assets.GetAssetBundle(chunks[0].bundle);
            CancellationToken.ThrowIfCancellationRequested();
        }

        public static WebStoryRuntimeSession Create(
            WebPlayerLaunchConfiguration configuration)
        {
            return new WebStoryRuntimeSession(configuration);
        }

        public bool Matches(WebPlayerLaunchConfiguration configuration)
        {
            return configuration != null
                && string.Equals(
                    Configuration.storyId,
                    configuration.storyId,
                    StringComparison.Ordinal)
                && string.Equals(
                    Configuration.storyVersion,
                    configuration.storyVersion,
                    StringComparison.Ordinal)
                && string.Equals(
                    Configuration.manifestUrl,
                    configuration.manifestUrl,
                    StringComparison.Ordinal);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            try
            {
                _cancellation.Cancel();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                try { Episode.Dispose(); }
                catch (Exception exception) { Debug.LogException(exception); }
                finally
                {
                    _cancellation.Dispose();
                    if (_runtimeRoot != null)
                        UnityEngine.Object.Destroy(_runtimeRoot);
                }
            }
        }
    }
}
