using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.Save
{
    internal sealed class SaveWriter : BaseDisposable
    {
        private readonly object _gate = new();
        private readonly object _writeGate = new();
        private readonly string _key;
        private readonly Action<string, byte[]> _write;
        private readonly Action<Exception> _onError;
        private readonly CancellationTokenSource _cancellation = new();
        private byte[] _pending;
        private bool _running;

        internal SaveWriter(
            string key,
            Action<string, byte[]> write,
            Action<Exception> onError)
        {
            _key = key;
            _write = write ?? throw new ArgumentNullException(nameof(write));
            _onError = onError;
        }

        internal void Enqueue(byte[] data)
        {
            lock (_gate)
            {
                _pending = data;
                if (_running)
                    return;
                _running = true;
            }
            Run().Forget();
        }

        internal void Reset(Action clearStorage)
        {
            lock (_gate)
                _pending = null;
            lock (_writeGate)
                clearStorage();
        }

        private async UniTaskVoid Run()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                byte[] data;
                lock (_gate)
                {
                    data = _pending;
                    _pending = null;
                    if (data == null)
                    {
                        _running = false;
                        return;
                    }
                }

                try
                {
                    await UniTask.SwitchToThreadPool();
                    lock (_writeGate)
                        _write(_key, data);
                    await UniTask.SwitchToMainThread();
                }
                catch (Exception exception)
                {
                    await UniTask.SwitchToMainThread();
                    _onError?.Invoke(exception);
                }
            }
        }

        protected override void OnDispose()
        {
            _cancellation.Cancel();
            byte[] pending;
            lock (_gate)
            {
                pending = _pending;
                _pending = null;
            }
            if (pending != null)
            {
                try
                {
                    lock (_writeGate)
                        _write(_key, pending);
                }
                catch (Exception exception)
                {
                    _onError?.Invoke(exception);
                }
            }
            _cancellation.Dispose();
            base.OnDispose();
        }
    }
}
