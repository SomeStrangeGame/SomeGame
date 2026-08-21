using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels.Bootstrap
{
    public sealed class BootstrapController : BaseDisposable
    {
        private readonly CancellationToken _cancellationToken;
        private View.BootstrapScreen _screen;

        public BootstrapController(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void ShowLoading(string message)
        {
            EnsureScreen();
            _screen.ShowLoading(message);
        }

        public async UniTask WaitForRetry(string message, string retryLabel)
        {
            EnsureScreen();
            var retry = new UniTaskCompletionSource();
            _screen.ShowRetry(
                message,
                retryLabel,
                () => retry.TrySetResult());
            await retry.Task.AttachExternalCancellation(_cancellationToken);
        }

        public void Hide()
        {
            if (_screen != null)
                _screen.gameObject.SetActive(false);
        }

        protected override void OnDispose()
        {
            if (_screen != null)
                UnityEngine.Object.Destroy(_screen.gameObject);
            _screen = null;
            base.OnDispose();
        }

        private void EnsureScreen()
        {
            if (_screen == null)
                _screen = View.BootstrapScreen.Create();
            _screen.gameObject.SetActive(true);
        }
    }
}
