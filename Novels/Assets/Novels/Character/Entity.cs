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
            public Func<string, string, string, string, string> GetHairPath;
            public Func<string, string, string, string> GetAccessoriesPath;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _wardrobe = "Wardrobe";
        private const string _child = "маленькая";

        private readonly Ctx _ctx;

        private View.Screen _screen;

        private string _mainCharacterView;
        private string _mainCharacterClothes;
        private string _currentCharacterClothes;
        private string _mainCharacterHair;
        private string _currentCharacterHair;
        private string _currentCharacterAccessories;


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

        public void SetMainCharacterClothes(string clothes)
        {
            _mainCharacterClothes = clothes;
            _currentCharacterClothes = null;
        }

        public void SetMainCharacterHair(string hair)
        {
            _mainCharacterHair = hair;
            _currentCharacterHair = null;
        }

        public async UniTask SetImage(string name, params string[] args)
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

            await UniTask.WhenAll(
                SetMainBody(name, view, args),
                SetEmotion(name, view, args),
                SetClothes(name, clothes, args),
                SetHairs(name, hair, args),
                SetAccessoiries(name, args));
        }

        public async UniTask Show(bool isLeft)
        {
            await _screen.ShowImage(isLeft);
        }

        public void ShowImmediate(bool isLeft)
        {
            _screen.ShowImageImmediate(isLeft);
        }

        private async UniTask SetMainBody(string name, string view, string[] args)
        {
            var mainBodySprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, null));
            foreach (var arg in args)
            {
                var customBody = arg;
                if (arg.ToLower() == _child)
                {
                    view = $"{view}/Child";
                    customBody = null;
                }
                var currentMainBodySprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, customBody));
                if (currentMainBodySprite != null)
                {
                    mainBodySprite = currentMainBodySprite;
                    break;
                }
            }
            _screen.SetMainBody(mainBodySprite);
        }

        private async UniTask SetEmotion(string name, string view, string[] args)
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
                var emotionSprite = await _ctx.GetSprite(_ctx.GetEmotionPath(name, view, emotion));
                if (emotionSprite != null)
                {
                    _screen.SetEmotion(emotionSprite);
                    break;
                }
            }
        }

        private async UniTask SetClothes(string name, string clothes, string[] args, int clothesIndex = 1)
        {
            foreach (var arg in args)
            {
                var customClothes = arg;
                if (customClothes.ToLower() == _child)
                {
                    clothes = null;
                    _currentCharacterClothes = null;
                }
                else if (customClothes.ToLower() == "убрать одежду")
                {
                    customClothes = null;
                    _currentCharacterClothes = null;
                }
                var sprite = await _ctx.GetSprite(_ctx.GetClothesPath(name, customClothes, clothesIndex));
                if (sprite != null) 
                {
                    _currentCharacterClothes = customClothes;
                    break;
                }
            }
            var clothesSprite = await _ctx.GetSprite(_ctx.GetClothesPath(name, _currentCharacterClothes ?? clothes, clothesIndex));
            _screen.SetClothes(clothesSprite);
        }

        private async UniTask SetHairs(string name, string hair, string[] args, string color = "Блонд")
        {
            foreach (var arg in args)
            {
                if (arg.ToLower() == _child)
                {
                    _currentCharacterHair = null;
                    hair = null;
                }
                else if (arg.ToLower() == "убрать причёску" || arg.ToLower() == "убрать прическу")
                {
                    _currentCharacterHair = null;
                }
                var backHairSprite = await _ctx.GetSprite(_ctx.GetHairPath(name, arg, "Back", color));
                if (backHairSprite != null) 
                {
                    _currentCharacterHair = arg;
                    break;
                }
                var frontHairSprite = await _ctx.GetSprite(_ctx.GetHairPath(name, arg, "Front", color));
                if (frontHairSprite != null) 
                {
                    _currentCharacterHair = arg;
                    break;
                }
            }
            _screen.SetBackHairs(await _ctx.GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, "Back", color)));
            _screen.SetFrontHairs(await _ctx.GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, "Front", color)));
        }

        private async UniTask SetAccessoiries(string name, string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower() == _child)
                {
                    _currentCharacterAccessories = null;
                }
                else if (arg.ToLower() == "убрать аксессуар")
                {
                    _currentCharacterAccessories = null;
                }
                var backAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, "Back"));
                if (backAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
                var middleAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, "Middle"));
                if (middleAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
                var frontAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, "Front"));
                if (frontAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
            }
            _screen.SetBackAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, "Back")));
            _screen.SetMiddleAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, "Middle")));
            _screen.SetFrontAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, "Front")));
        }

        public async UniTask Hide()
        {
            await _screen.HideImage();
        }

        public void HideImmediate()
        {
            _screen.HideImageImmediate();
        }
    }
}

