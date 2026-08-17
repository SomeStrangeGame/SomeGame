using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Notification
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject NotificationPrefab;
            public CancellationToken CancellationToken;
            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Queue<string> _pendingNotifications = new();
        private bool _isProcessing;

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenGO = GameObject.Instantiate(_ctx.NotificationPrefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public void Enqueue(string text)
        {
            _pendingNotifications.Enqueue(text ?? string.Empty);
            if (!_isProcessing)
                ProcessQueue().Forget();
        }

        private async UniTaskVoid ProcessQueue()
        {
            _isProcessing = true;
            try
            {
                while (_pendingNotifications.TryDequeue(out var text))
                {
                    _ctx.CancellationToken.ThrowIfCancellationRequested();
                    _screen.SetText(text);

                    await _screen.Show(_ctx.CancellationToken);
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(3),
                        cancellationToken: _ctx.CancellationToken);
                    await _screen.Hide(_ctx.CancellationToken);
                }
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.NotificationFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    "Notification processing failed.",
                    exception: exception));
            }
            finally
            {
                _isProcessing = false;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _pendingNotifications.Clear();
            _isProcessing = false;
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
