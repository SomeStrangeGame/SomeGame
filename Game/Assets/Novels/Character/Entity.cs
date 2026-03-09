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
            if (view == "Азиатская")
                view = "Asia";
            else if (view == "Европейская")
                view = "Euro";
            _mainCharacterView = $"View/{view}";
        }

        public async UniTask SetImage(string name, params string[] args)
        {
            var view = "View";
            if (name == "Салли")
            {
                name = "MainCharacter";
                view = _mainCharacterView;
            }
            else if (name == "Бен")
                name = "Ben";

            await Hide();
            var sprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view));
            Debug.Log($"{_ctx.GetMainBodyPath(name, view)} - {sprite != null}");
            _screen.SetMainBody(sprite);

            _screen.SetEmotion(null);
            foreach (var arg in args)
            {
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

