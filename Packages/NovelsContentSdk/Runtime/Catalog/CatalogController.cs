using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Catalog
{
    public sealed class CatalogController : BaseDisposable
    {
        private readonly GameObject _bundledPrefab;
        private readonly CancellationToken _cancellationToken;
        private View.CatalogScreen _screen;

        public CatalogController(
            GameObject bundledPrefab,
            CancellationToken cancellationToken)
        {
            _bundledPrefab = bundledPrefab
                ?? throw new ArgumentNullException(nameof(bundledPrefab));
            _cancellationToken = cancellationToken;
        }

        public async UniTask<CatalogItem> Select(
            string title,
            IReadOnlyList<CatalogItem> items)
        {
            if (items == null || items.Count == 0)
                throw new InvalidOperationException("Catalog is empty.");

            EnsureScreen();
            _screen.SetTitle(title);
            var selection = new UniTaskCompletionSource<CatalogItem>();
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id))
                    throw new InvalidOperationException(
                        "Catalog contains an item without an id.");
                _screen.AddOrUpdateCard(
                    item.Id,
                    item.Title,
                    item.Genre,
                    item.Description,
                    item.Status,
                    item.ActionLabel,
                    item.IsEnabled,
                    item.Cover,
                    () => selection.TrySetResult(item));
            }

            try
            {
                _screen.gameObject.SetActive(true);
                return await selection.Task.AttachExternalCancellation(
                    _cancellationToken);
            }
            finally
            {
                if (_screen != null)
                    _screen.gameObject.SetActive(false);
            }
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
            if (_screen != null)
                return;
            var instance = UnityEngine.Object.Instantiate(_bundledPrefab);
            _screen = instance.GetComponent<View.CatalogScreen>();
            if (_screen == null)
            {
                UnityEngine.Object.Destroy(instance);
                throw new InvalidOperationException(
                    "Catalog prefab does not contain Catalog.View.CatalogScreen.");
            }
        }
    }
}
