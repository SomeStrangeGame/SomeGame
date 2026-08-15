using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Character
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject ScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
            public Func<string, string, int, string> GetClothesPath;
            public Func<string, string, string, string, string> GetHairPath;
            public Func<string, string, string, string> GetAccessoriesPath;
            public CancellationToken CancellationToken;
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

        public async UniTask SetImage(StoryContracts.CharacterRenderRequest request)
        {
            var name = request.Name;
            var presentation = request.Presentation;
            var view = "View";
            var clothes = string.Empty;
            var hair = string.Empty;
            if (request.Role == StoryContracts.StorySpeakerRole.MainCharacter
                || request.Role == StoryContracts.StorySpeakerRole.Wardrobe)
            {
                name = _mainCharacter;
                view = _mainCharacterView;
                clothes = _mainCharacterClothes;
                hair = _mainCharacterHair;
            }

            await UniTask.WhenAll(
                SetMainBody(name, view, presentation),
                SetEmotion(name, view, presentation),
                SetClothes(name, clothes, presentation),
                SetHairs(name, hair, presentation),
                SetAccessoiries(name, presentation));
        }

        public async UniTask Show(StoryContracts.StoryCharacterPosition position)
        {
            await _screen.ShowImage(ToViewPosition(position), _ctx.CancellationToken);
        }

        public void ShowImmediate(StoryContracts.StoryCharacterPosition position)
        {
            _screen.ShowImageImmediate(ToViewPosition(position));
        }

        private static bool? ToViewPosition(StoryContracts.StoryCharacterPosition position)
        {
            return position switch
            {
                StoryContracts.StoryCharacterPosition.Left => true,
                StoryContracts.StoryCharacterPosition.Right => false,
                _ => null,
            };
        }

        private UniTask<Sprite> GetSprite(string path)
        {
            return _ctx.GetSprite(path).AttachExternalCancellation(_ctx.CancellationToken);
        }

        private async UniTask SetMainBody(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            var mainBodySprite = await GetSprite(_ctx.GetMainBodyPath(name, view, null));
            if (presentation.IsChild)
            {
                view = $"{view}/{_childView}";
                var childBodySprite = await GetSprite(_ctx.GetMainBodyPath(name, view, null));
                if (childBodySprite != null)
                    mainBodySprite = childBodySprite;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                var currentMainBodySprite = await GetSprite(_ctx.GetMainBodyPath(name, view, candidate));
                if (currentMainBodySprite != null)
                {
                    mainBodySprite = currentMainBodySprite;
                    break;
                }
            }
            _screen.SetMainBody(mainBodySprite);
        }

        private async UniTask SetEmotion(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            _screen.SetEmotion(null);
            if (presentation.IsChild)
                view = $"{view}/{_childView}";

            foreach (var candidate in presentation.AssetCandidates)
            {
                var emotionSprite = await GetSprite(_ctx.GetEmotionPath(name, view, candidate));
                if (emotionSprite != null)
                {
                    _screen.SetEmotion(emotionSprite);
                    break;
                }
            }
        }

        private async UniTask SetClothes(
            string name,
            string clothes,
            StoryContracts.CharacterPresentation presentation,
            int clothesIndex = 1)
        {
            if (presentation.IsChild)
            {
                clothes = null;
                _currentCharacterClothes = null;
            }
            else if (presentation.RemoveClothes)
            {
                _currentCharacterClothes = null;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(_ctx.GetClothesPath(name, candidate, clothesIndex));
                if (sprite != null) 
                {
                    _currentCharacterClothes = candidate;
                    break;
                }
            }
            var clothesSprite = await GetSprite(_ctx.GetClothesPath(name, _currentCharacterClothes ?? clothes, clothesIndex));
            _screen.SetClothes(clothesSprite);
        }

        private async UniTask SetHairs(
            string name,
            string hair,
            StoryContracts.CharacterPresentation presentation,
            string color = _defaultHairColor)
        {
            if (presentation.IsChild)
            {
                _currentCharacterHair = null;
                hair = null;
            }
            else if (presentation.RemoveHair)
            {
                _currentCharacterHair = null;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                var backHairSprite = await GetSprite(_ctx.GetHairPath(name, candidate, _backLayer, color));
                if (backHairSprite != null) 
                {
                    _currentCharacterHair = candidate;
                    break;
                }
                var frontHairSprite = await GetSprite(_ctx.GetHairPath(name, candidate, _frontLayer, color));
                if (frontHairSprite != null) 
                {
                    _currentCharacterHair = candidate;
                    break;
                }
            }
            _screen.SetBackHairs(await GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, _backLayer, color)));
            _screen.SetFrontHairs(await GetSprite(_ctx.GetHairPath(name, _currentCharacterHair ?? hair, _frontLayer, color)));
        }

        private async UniTask SetAccessoiries(
            string name,
            StoryContracts.CharacterPresentation presentation)
        {
            if (presentation.IsChild || presentation.RemoveAccessory)
                _currentCharacterAccessories = null;

            foreach (var candidate in presentation.AssetCandidates)
            {
                var backAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _backLayer));
                if (backAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = candidate;
                    break;
                }
                var middleAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _middleLayer));
                if (middleAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = candidate;
                    break;
                }
                var frontAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _frontLayer));
                if (frontAccessoriesSprite != null) 
                {
                    _currentCharacterAccessories = candidate;
                    break;
                }
            }
            _screen.SetBackAccessories(await GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _backLayer)));
            _screen.SetMiddleAccessories(await GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _middleLayer)));
            _screen.SetFrontAccessories(await GetSprite(_ctx.GetAccessoriesPath(name, _currentCharacterAccessories, _frontLayer)));
        }

        public async UniTask Hide()
        {
            await _screen.HideImage(_ctx.CancellationToken);
        }

        public void HideImmediate()
        {
            _screen.HideImageImmediate();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
