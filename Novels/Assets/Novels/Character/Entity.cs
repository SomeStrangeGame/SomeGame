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
            public Func<GameObject> GetScreenPrefab;
            public Func<string, Sprite> GetSprite;
            public Func<string, string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
            public Func<string, string, int, string> GetClothesPath;
            public Func<string, string, string, string, string> GetHairSprite;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _wardrobe = "Wardrobe";
        private const string _child = "маленькая";

        private readonly Ctx _ctx;

        private View.Screen _screen;

        private string _mainCharacterView;
        private string _mainCharacterClothes;
        private string _mainCharacterHair;


        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            var prefab = _ctx.GetScreenPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImageImmediate();
        }

        public void SetMainCharacterView(string view)
        {
            _mainCharacterView = $"View/{view}";
        }

        public void SetMainCharacterClothes(string clothes)
        {
            _mainCharacterClothes = clothes;
        }

        public void SetMainCharacterHair(string hair)
        {
            _mainCharacterHair = hair;
        }

        public async UniTask SetImageAndShow(bool isLoading, string name, params string[] args)
        {
            var view = "View";
            var clothes = string.Empty;
            var hair = string.Empty;
            var isLeft = name == _ctx.MainCharacterName;
            if (isLeft || name == _wardrobe)
            {
                name = _mainCharacter;
                view = _mainCharacterView;
                clothes = _mainCharacterClothes;
                hair = _mainCharacterHair;
            }

            SetMainBody(name, view, args);
            SetEmotion(name, view, args);
            SetClothes(name, clothes, args);
            SetHairs(name, hair, args);

            if (isLoading)
                _screen.ShowImageImmediate();
            else
                await _screen.ShowImage(isLeft);
        }

        private void SetMainBody(string name, string view, string[] args)
        {
            var defaultMainBodySprite = _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, null));
            _screen.SetMainBody(defaultMainBodySprite);
            foreach (var arg in args)
            {
                var customBody = arg;
                if (arg.ToLower() == _child)
                {
                    view = $"{view}/Child";
                    customBody = null;
                }
                var mainBodySprite = _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, customBody));
                if (mainBodySprite != null)
                {
                    _screen.SetMainBody(mainBodySprite);
                    break;
                }
            }
        }

        private void SetEmotion(string name, string view, string[] args)
        {
            _screen.SetEmotion(null);
            foreach (var arg in args)
            {
                var emotion = arg;
                if (arg.ToLower() == _child)
                {
                    view = $"{view}/Child";
                    emotion = null;
                }
                var emotionSprite = _ctx.GetSprite(_ctx.GetEmotionPath(name, view, emotion));
                if (emotionSprite != null)
                {
                    _screen.SetEmotion(emotionSprite);
                    break;
                }
            }
        }

        private void SetClothes(string name, string clothes, string[] args, int clothesIndex = 1)
        {
            var defaultClothesSprite = _ctx.GetSprite(_ctx.GetClothesPath(name, clothes, clothesIndex));
            _screen.SetClothes(defaultClothesSprite);
            foreach (var arg in args)
            {
                if (arg.ToLower() == _child)
                {
                    _screen.SetClothes(null);
                }
                var clothesSprite = _ctx.GetSprite(_ctx.GetClothesPath(name, arg, clothesIndex));
                if (clothesSprite != null) 
                {
                    _screen.SetClothes(clothesSprite);
                    break;
                }
            }
        }

        private void SetHairs(string name, string hair, string[] args, string color = "Блонд")
        {
            var defaultBackHairSprite = _ctx.GetSprite(_ctx.GetHairSprite(name, hair, "Back", color));
            _screen.SetBackHairs(defaultBackHairSprite);
            foreach (var arg in args)
            {
                if (arg.ToLower() == _child)
                {
                    _screen.SetBackHairs(null);
                }
                var backHairSprite = _ctx.GetSprite(_ctx.GetHairSprite(name, arg, "Back", color));
                if (backHairSprite != null) 
                {
                    _screen.SetBackHairs(backHairSprite);
                    break;
                }
            }
            var defaultFrontHairSprite = _ctx.GetSprite(_ctx.GetHairSprite(name, hair, "Front", color));
            _screen.SetFrontHairs(defaultFrontHairSprite);
            foreach (var arg in args)
            {
                if (arg.ToLower() == _child)
                {
                    _screen.SetFrontHairs(null);
                }
                var frontHairSprite = _ctx.GetSprite(_ctx.GetHairSprite(name, arg, "Front", color));
                if (frontHairSprite != null) 
                {
                    _screen.SetFrontHairs(frontHairSprite);
                    break;
                }
            }
        }

        public async UniTask Hide(bool isLoading)
        {
            if (isLoading)
                _screen.HideImageImmediate();
            else
                await _screen.HideImage();
        }
    }
}

