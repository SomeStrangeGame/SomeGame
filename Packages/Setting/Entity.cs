using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Setting
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<GameObject> GetBundledPrefab;
        }

        private View.Screen _screen;
        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var screenPrefabGO = _ctx.GetBundledPrefab();
            var screenGO = GameObject.Instantiate(screenPrefabGO);

            _screen = screenGO.GetComponent<View.Screen>();
        }

        public void Show()
        {
            _screen.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _screen.gameObject.SetActive(false);
        }

        public void AddOrUpdateButton(string id, string text, Action onClick)
        {
            _screen.AddOrUpdateButton(id, text, onClick);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null) GameObject.Destroy(_screen.gameObject);
        }
    }
}
