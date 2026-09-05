using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.OptionSelection
{
    public enum OptionListLayout
    {
        Default,
        Wardrobe,
    }

    public sealed class OptionListController
    {
        private readonly CancellationToken _cancellationToken;
        private readonly OptionListScreen _screenPrefab;
        private OptionListScreen _screen;

        public OptionListController(
            CancellationToken cancellationToken,
            OptionListScreen screenPrefab = null)
        {
            _cancellationToken = cancellationToken;
            _screenPrefab = screenPrefab;
        }

        public void Init(
            string name,
            OptionListLayout layout = OptionListLayout.Default,
            System.Action<int> selectTab = null)
        {
            var resourceName = layout == OptionListLayout.Wardrobe
                ? "WardrobeScreen"
                : "ChoiceScreen";
            var prefab = _screenPrefab != null
                ? _screenPrefab
                : Resources.Load<OptionListScreen>(resourceName);
            if (prefab == null)
            {
                throw new System.InvalidOperationException(
                    $"{resourceName} prefab is missing from Resources.");
            }
            _screen = Object.Instantiate(prefab);
            _screen.name = name;
            _screen.ConfigureLayout(layout, selectTab);
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
