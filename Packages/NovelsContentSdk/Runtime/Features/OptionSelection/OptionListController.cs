using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.OptionSelection
{
    public sealed class OptionListController
    {
        private readonly CancellationToken _cancellationToken;
        private OptionListScreen _screen;

        public OptionListController(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public void Init(string name)
        {
            var screenObject = new GameObject(name);
            _screen = screenObject.AddComponent<OptionListScreen>();
            _screen.Init();
        }

        public void Present(OptionListPresentation presentation) =>
            _screen.SetPresentation(presentation);

        public UniTask Show()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _screen.ShowImmediate();
            return UniTask.CompletedTask;
        }

        public UniTask Hide()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _screen.HideImmediate();
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            if (_screen != null)
                Object.Destroy(_screen.gameObject);
            _screen = null;
        }
    }
}
