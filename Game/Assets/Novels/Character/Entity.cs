using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Character
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<UniTask<GameObject>> GetScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
        }

        private readonly Ctx _ctx;

        private View.Screen _screen;

        private string _mainCharacterView;


        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var prefab = await _ctx.GetScreenPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImageImmediate();
        }

        public void SetMainCharacterView(string view)
        {
            _mainCharacterView = $"Внешность/{view}";
        }

        public async UniTask SetImage(string name, params string[] args)
        {
            var view = "Внешность";
            if (name == "Салли")
            {
                view = _mainCharacterView;
            }

            await Hide();
            Debug.Log(_ctx.GetMainBodyPath(name, view));
            var sprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view));
            _screen.SetMainBody(sprite);

            _screen.SetEmotion(null);
            foreach (var arg in args)
            {
                Debug.Log(_ctx.GetEmotionPath(name, view, arg));
                var emotionSprite = await _ctx.GetSprite(_ctx.GetEmotionPath(name, view, arg));
                _screen.SetEmotion(emotionSprite);
                if (emotionSprite != null) break;
            }
            await Show();
        }

        public async UniTask Show()
        {
            await _screen.ShowImage();
        }

        public async UniTask Hide()
        {
            await _screen.HideImage();
        }
    }
}

