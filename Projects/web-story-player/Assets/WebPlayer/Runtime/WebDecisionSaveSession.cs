using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.WebPlayer
{
    /// <summary>
    /// Adapts the shared binary decision save to IndexedDB. Open before composing
    /// the episode; await Decisions.FlushAsync at checkpoints and before disposal.
    /// </summary>
    public sealed class WebDecisionSaveSession : BaseDisposable
    {
        private readonly object _gate = new();
        private readonly WebStorySaveStore _store;
        private readonly string _key;
        private readonly string _releaseVersion;
        private byte[] _snapshot;
        private string _pending;
        private bool _hasPending;
        private bool _running;
        private UniTaskCompletionSource _idle;
        private Exception _failure;

        private WebDecisionSaveSession(WebStorySaveStore store, string storyId,
            string releaseVersion, string contentVersion, string episodeId, byte[] snapshot,
            Action<StoryRuntimeError> onError)
        {
            _store = store;
            _key = StorageKey(storyId, episodeId);
            _releaseVersion = releaseVersion;
            _snapshot = snapshot;
            Decisions = new Save.SaveSystem(new Save.SaveSystem.Dependencies
            {
                SaveChoiceFileName = _key,
                ContentId = storyId + "/" + episodeId,
                ContentVersion = contentVersion,
                ReadBytes = _ => ReadSnapshot(),
                WriteBytes = (_, bytes) => StoreSnapshot(bytes),
                Delete = _ => StoreSnapshot(null),
                FlushStorageAsync = FlushStorage,
                OnError = onError,
            }).AddTo(this);
        }

        public Save.SaveSystem Decisions { get; }

        public static async UniTask<WebDecisionSaveSession> Open(WebStorySaveStore store,
            string storyId, string releaseVersion, string contentVersion, string episodeId,
            Action<StoryRuntimeError> onError = null)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrWhiteSpace(storyId) || string.IsNullOrWhiteSpace(episodeId)
                || string.IsNullOrWhiteSpace(releaseVersion) || string.IsNullOrWhiteSpace(contentVersion))
                throw new ArgumentException("Story, episode and both versions are required.");
            var encoded = await store.Read(StorageKey(storyId, episodeId), releaseVersion);
            var snapshot = encoded == null ? null : Convert.FromBase64String(encoded);
            var session = new WebDecisionSaveSession(store, storyId, releaseVersion,
                contentVersion, episodeId, snapshot, onError);
            // Browser storage errors/corruption must not silently reset progress.
            if (snapshot != null && !session.Decisions.TryReadCompatibleDecisions(snapshot, out _))
            {
                session.Dispose();
                throw new InvalidDataException("Stored decisions are incompatible or damaged; save preserved.");
            }
            session.Decisions.Init();
            return session;
        }

        private static string StorageKey(string storyId, string episodeId) =>
            Uri.EscapeDataString(storyId) + "/" + Uri.EscapeDataString(episodeId);

        private byte[] ReadSnapshot()
        {
            lock (_gate)
                return _snapshot == null ? throw new FileNotFoundException()
                    : (byte[])_snapshot.Clone();
        }

        private void StoreSnapshot(byte[] bytes)
        {
            var start = false;
            lock (_gate)
            {
                _snapshot = bytes == null ? null : (byte[])bytes.Clone();
                _pending = bytes == null ? null : Convert.ToBase64String(bytes);
                _hasPending = true;
                if (!_running)
                {
                    _running = true;
                    _idle = new UniTaskCompletionSource();
                    start = true;
                }
            }
            if (start) Drain().Forget();
        }

        private async UniTask Drain()
        {
            // SaveWriter can call us from its native/Editor worker thread.
            await UniTask.SwitchToMainThread();
            while (true)
            {
                string value = null;
                UniTaskCompletionSource finished = null;
                lock (_gate)
                {
                    if (!_hasPending)
                    {
                        _running = false;
                        finished = _idle;
                    }
                    else
                    {
                        value = _pending;
                        _pending = null;
                        _hasPending = false;
                    }
                }
                if (finished != null)
                {
                    finished.TrySetResult();
                    return;
                }
                Exception failure = null;
                try
                {
                    if (value == null) await _store.Delete(_key, _releaseVersion);
                    else await _store.Write(_key, _releaseVersion, value);
                }
                catch (Exception exception) { failure = exception; }
                lock (_gate) _failure = failure;
            }
        }

        private async UniTask FlushStorage()
        {
            while (true)
            {
                UniTask wait;
                lock (_gate)
                {
                    if (!_running)
                    {
                        if (_failure != null)
                            throw new IOException("Browser save was not committed.", _failure);
                        return;
                    }
                    wait = _idle.Task;
                }
                await wait;
            }
        }

        public async UniTask RetryFlushAsync()
        {
            // First wait for any new decision to reach this adapter, even if the
            // previous database transaction failed. Never re-enqueue an older save.
            try { await Decisions.FlushAsync(); }
            catch (IOException)
            {
                lock (_gate) StoreSnapshot(_snapshot);
                await FlushStorage();
            }
        }
    }
}
