using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Character
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string MainCharacterName;
            public Func<UniTask<GameObject>> GetScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
            public Func<string, string, int, string> GetClothesPath;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _wardrobe = "Wardrobe";

        private readonly Ctx _ctx;

        private View.Screen _screen;

        private string _mainCharacterView;
        private string _mainCharacterClothes;


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
            _mainCharacterView = $"View/{view}";
        }

        public void SetMainCharacterWeather(string weather)
        {
            _mainCharacterClothes = weather;
        }

        public async UniTask SetImageAndShow(string name, params string[] args)
        {
            var view = "View";
            var clothes = string.Empty;
            if (name == _ctx.MainCharacterName || name == _wardrobe)
            {
                name = _mainCharacter;
                view = _mainCharacterView;
                clothes = _mainCharacterClothes;
            }

            await UniTask.WhenAll(
                SetMainBody(name, view, args),
                SetEmotion(name, view, args),
                SetClothes(name, clothes, args));

            await _screen.ShowImage();
        }

        private async UniTask SetMainBody(string name, string view, string[] args)
        {
            var defaultMainBodySprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, null));
            _screen.SetMainBody(defaultMainBodySprite);
            foreach (var arg in args)
            {
                var mainBodySprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, arg));
                if (mainBodySprite != null)
                {
                    _screen.SetMainBody(mainBodySprite);
                    break;
                }
            }
        }

        private async UniTask SetEmotion(string name, string view, string[] args)
        {
            _screen.SetEmotion(null);
            foreach (var arg in args)
            {
                var emotionSprite = await _ctx.GetSprite(_ctx.GetEmotionPath(name, view, arg));
                if (emotionSprite != null)
                {
                    _screen.SetEmotion(emotionSprite);
                    break;
                }
            }
        }

        private async UniTask SetClothes(string name, string clothes, string[] args, int clothesIndex = 1)
        {
            var defaultClothesSprite = await _ctx.GetSprite(_ctx.GetClothesPath(name, clothes, clothesIndex));
            _screen.SetClothes(defaultClothesSprite);
            foreach (var arg in args)
            {
                var clothesSprite = await _ctx.GetSprite(_ctx.GetClothesPath(name, arg, clothesIndex));
                if (clothesSprite != null) 
                {
                    _screen.SetClothes(clothesSprite);
                    break;
                }
            }
        }

        public async UniTask Hide()
        {
            await _screen.HideImage();
        }
    }
}

