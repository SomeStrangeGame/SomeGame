using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, CharacterAppearanceState> _appearanceByCharacter = new();

        private string _mainCharacterView;
        private string _mainCharacterClothes;
        private string _mainCharacterHair;


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
            GetAppearanceState(_mainCharacter).Clothes = null;
        }

        public void SetMainCharacterHair(string hair)
        {
            _mainCharacterHair = hair;
            GetAppearanceState(_mainCharacter).Hair = null;
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

            var appearance = GetAppearanceState(name ?? string.Empty);

            var (mainBody, emotion, clothesSprite, hairSprites, accessorySprites) = await UniTask.WhenAll(
                ResolveMainBody(name, view, presentation),
                ResolveEmotion(name, view, presentation),
                ResolveClothes(name, clothes, presentation, appearance),
                ResolveHair(name, hair, presentation, appearance),
                ResolveAccessories(name, presentation, appearance));

            Apply(new CharacterSpriteSet(
                mainBody,
                emotion,
                clothesSprite,
                hairSprites,
                accessorySprites));
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

        private CharacterAppearanceState GetAppearanceState(string identity)
        {
            if (_appearanceByCharacter.TryGetValue(identity, out var appearance))
                return appearance;

            appearance = new CharacterAppearanceState();
            _appearanceByCharacter.Add(identity, appearance);
            return appearance;
        }

        private async UniTask<Sprite> ResolveMainBody(
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
            return mainBodySprite;
        }

        private async UniTask<Sprite> ResolveEmotion(
            string name,
            string view,
            StoryContracts.CharacterPresentation presentation)
        {
            if (presentation.IsChild)
                view = $"{view}/{_childView}";

            foreach (var candidate in presentation.AssetCandidates)
            {
                var emotionSprite = await GetSprite(_ctx.GetEmotionPath(name, view, candidate));
                if (emotionSprite != null)
                    return emotionSprite;
            }

            return null;
        }

        private async UniTask<Sprite> ResolveClothes(
            string name,
            string clothes,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance,
            int clothesIndex = 1)
        {
            if (presentation.IsChild)
            {
                clothes = null;
                appearance.Clothes = null;
            }
            else if (presentation.RemoveClothes)
            {
                appearance.Clothes = null;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                var sprite = await GetSprite(_ctx.GetClothesPath(name, candidate, clothesIndex));
                if (sprite != null) 
                {
                    appearance.Clothes = candidate;
                    break;
                }
            }
            return await GetSprite(_ctx.GetClothesPath(name, appearance.Clothes ?? clothes, clothesIndex));
        }

        private async UniTask<CharacterHairSprites> ResolveHair(
            string name,
            string hair,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance,
            string color = _defaultHairColor)
        {
            if (presentation.IsChild)
            {
                appearance.Hair = null;
                hair = null;
            }
            else if (presentation.RemoveHair)
            {
                appearance.Hair = null;
            }

            foreach (var candidate in presentation.AssetCandidates)
            {
                var backHairSprite = await GetSprite(_ctx.GetHairPath(name, candidate, _backLayer, color));
                if (backHairSprite != null) 
                {
                    appearance.Hair = candidate;
                    break;
                }
                var frontHairSprite = await GetSprite(_ctx.GetHairPath(name, candidate, _frontLayer, color));
                if (frontHairSprite != null) 
                {
                    appearance.Hair = candidate;
                    break;
                }
            }
            var resolvedHair = appearance.Hair ?? hair;
            var (back, front) = await UniTask.WhenAll(
                GetSprite(_ctx.GetHairPath(name, resolvedHair, _backLayer, color)),
                GetSprite(_ctx.GetHairPath(name, resolvedHair, _frontLayer, color)));
            return new CharacterHairSprites(back, front);
        }

        private async UniTask<CharacterAccessorySprites> ResolveAccessories(
            string name,
            StoryContracts.CharacterPresentation presentation,
            CharacterAppearanceState appearance)
        {
            if (presentation.IsChild || presentation.RemoveAccessory)
                appearance.Accessories = null;

            foreach (var candidate in presentation.AssetCandidates)
            {
                var backAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _backLayer));
                if (backAccessoriesSprite != null) 
                {
                    appearance.Accessories = candidate;
                    break;
                }
                var middleAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _middleLayer));
                if (middleAccessoriesSprite != null) 
                {
                    appearance.Accessories = candidate;
                    break;
                }
                var frontAccessoriesSprite = await GetSprite(_ctx.GetAccessoriesPath(name, candidate, _frontLayer));
                if (frontAccessoriesSprite != null) 
                {
                    appearance.Accessories = candidate;
                    break;
                }
            }
            var (back, middle, front) = await UniTask.WhenAll(
                GetSprite(_ctx.GetAccessoriesPath(name, appearance.Accessories, _backLayer)),
                GetSprite(_ctx.GetAccessoriesPath(name, appearance.Accessories, _middleLayer)),
                GetSprite(_ctx.GetAccessoriesPath(name, appearance.Accessories, _frontLayer)));
            return new CharacterAccessorySprites(back, middle, front);
        }

        private void Apply(CharacterSpriteSet sprites)
        {
            _screen.SetMainBody(sprites.MainBody);
            _screen.SetEmotion(sprites.Emotion);
            _screen.SetClothes(sprites.Clothes);
            _screen.SetBackHairs(sprites.Hair.Back);
            _screen.SetFrontHairs(sprites.Hair.Front);
            _screen.SetBackAccessories(sprites.Accessories.Back);
            _screen.SetMiddleAccessories(sprites.Accessories.Middle);
            _screen.SetFrontAccessories(sprites.Accessories.Front);
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
