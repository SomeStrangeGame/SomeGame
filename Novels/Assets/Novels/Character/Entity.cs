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
            public GameObject ScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
            public Func<string, string, int, string> GetClothesPath;
            public Func<string, string, string, string, string> GetHairPath;
            public Func<string, string, string, string> GetAccessoriesPath;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _childView = "Child";
        private const string _backLayer = "Back";
        private const string _middleLayer = "Middle";
        private const string _frontLayer = "Front";
        private const string _defaultHairColor = "Блонд";

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

        public void Init()
        {
            var screenGO = GameObject.Instantiate(_ctx.ScreenPrefab);
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
            if (isLeft || name == StoryContracts.StorySpeakers.Wardrobe)
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

        public async UniTask Show(bool? isLeft)
        {
            await _screen.ShowImage(isLeft);
        }

        public void ShowImmediate(bool? isLeft)
        {
            _screen.ShowImageImmediate(isLeft);
        }

        private async UniTask SetMainBody(string name, string view, string[] args)
        {
            var mainBodySprite = await _ctx.GetSprite(_ctx.GetMainBodyPath(name, view, null));
            foreach (var arg in args)
            {
                var customBody = arg;
                if (string.Equals(arg, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase))
                {
                    view = $"{view}/{_childView}";
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
                if (string.Equals(arg, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase))
                {
                    view = $"{view}/{_childView}";
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
                if (string.Equals(customClothes, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase))
                {
                    clothes = null;
                    _currentCharacterClothes = null;
                }
                else if (string.Equals(customClothes, StoryContracts.StoryArguments.RemoveClothes, StringComparison.OrdinalIgnoreCase))
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

        private async UniTask SetHairs(string name, string hair, string[] args, string color = _defaultHairColor)
        {
            foreach (var arg in args)
            {
                if (string.Equals(arg, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCharacterHair = null;
                    hair = null;
                }
                else if (string.Equals(arg, StoryContracts.StoryArguments.RemoveHair, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(arg, StoryContracts.StoryArguments.RemoveHairLegacy, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCharacterHair = null;
                }
                var backHairSprite = await _ctx.GetSprite(_ctx.GetHairPath(name, arg, _backLayer, color));
                if (backHairSprite != null) 
                {
                    _currentCharacterHair = arg;
                    break;
                }
                var frontHairSprite = await _ctx.GetSprite(_ctx.GetHairPath(name, arg, _frontLayer, color));
                if (frontHairSprite != null) 
                {
                    _currentCharacterHair = arg;
                    break;
                }
            }
            _screen.SetBackHairs(await _ctx.GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, _backLayer, color)));
            _screen.SetFrontHairs(await _ctx.GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, _frontLayer, color)));
        }

        private async UniTask SetAccessoiries(string name, string[] args)
        {
            foreach (var arg in args)
            {
                if (string.Equals(arg, StoryContracts.StoryArguments.Child, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCharacterAccessories = null;
                }
                else if (string.Equals(arg, StoryContracts.StoryArguments.RemoveAccessory, StringComparison.OrdinalIgnoreCase))
                {
                    _currentCharacterAccessories = null;
                }
                var backAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, _backLayer));
                if (backAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
                var middleAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, _middleLayer));
                if (middleAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
                var frontAccessoriesSprite = await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, arg, _frontLayer));
                if (frontAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = arg;
                    break;
                }
            }
            _screen.SetBackAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _backLayer)));
            _screen.SetMiddleAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _middleLayer)));
            _screen.SetFrontAccessories(await _ctx.GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _frontLayer)));
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
