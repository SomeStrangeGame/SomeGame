using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Location
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<UniTask<GameObject>> GetScreenPrefab;
            public Func<string, string, UniTask<Sprite>> GetSprite;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var prefab = await _ctx.GetScreenPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public async UniTask SetImage(string bundleName, string assetName)
        {
            await Hide();
            var sprite = await _ctx.GetSprite(bundleName, assetName);
            _screen.SetImage(sprite);
            await Show();
        }

        public async UniTask Show()
        {
            await _screen.Show();
        }

        public async UniTask Hide()
        {
            await _screen.Hide();
        }
    }
}

