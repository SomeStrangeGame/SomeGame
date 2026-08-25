using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Character
{
    public class CharacterController : BaseDisposable
    {
        public struct Dependencies
        {
            public GameObject ScreenPrefab;
            public string ContentPrefix;
            public Content.CharacterAssetProfile AssetProfile;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, UniTask<Sprite>> GetFullQualitySprite;
            public Func<UniTask<CharacterSpriteTrimManifest>> GetSpriteTrimManifest;
            public Sprite MissingCharacter;
            public CancellationToken CancellationToken;
        }

        private readonly Dependencies _ctx;
        private readonly CharacterSpriteResolver _spriteResolver;
        private readonly Content.CharacterAssetProfile _assetProfile;
        private View.CharacterScreen _screen;
        private string _mainCharacterView;
        private string _mainCharacterClothes;
        private string _mainCharacterHair;
        private string _mainCharacterAccessory;
        private StoryContracts.CharacterRenderRequest _lastRenderRequest;
        private int _wardrobePreviewVersion;

        public CharacterController(Dependencies ctx)
        {
            _ctx = ctx;
            _assetProfile = ctx.AssetProfile
                ?? throw new ArgumentNullException(nameof(ctx.AssetProfile));
            _spriteResolver = new CharacterSpriteResolver(
                ctx.ContentPrefix,
                _assetProfile,
                ctx.GetSprite,
                ctx.GetFullQualitySprite,
                ctx.GetSpriteTrimManifest,
                ctx.MissingCharacter,
                ctx.CancellationToken);
        }

        public void Init()
        {
            var screenGO = GameObject.Instantiate(_ctx.ScreenPrefab);
            _screen = screenGO.GetComponent<View.CharacterScreen>();
            _screen.HideImageImmediate();
        }

        public void SetMainCharacterView(string view)
        {
            _mainCharacterView = _assetProfile.ViewPath(view);
        }

        public void SetMainCharacterClothes(string clothes)
        {
            _mainCharacterClothes = clothes;
            _spriteResolver.ClearClothes();
        }

        public void SetMainCharacterHair(string hair)
        {
            _mainCharacterHair = hair;
            _spriteResolver.ClearHair();
        }

        public void SetMainCharacterAccessory(string accessory)
        {
            _mainCharacterAccessory = accessory;
            _spriteResolver.ClearAccessories();
        }

        public async UniTask SetImage(StoryContracts.CharacterRenderRequest request)
        {
            _wardrobePreviewVersion++;
            _lastRenderRequest = request;
            Apply(await _spriteResolver.Resolve(
                request,
                _mainCharacterView,
                _mainCharacterClothes,
                _mainCharacterHair,
                _mainCharacterAccessory));
        }

        public async UniTask EnableFullQuality()
        {
            if (_lastRenderRequest == null)
                return;
            _spriteResolver.EnableFullQuality();
            _spriteResolver.ClearLoadedSprites();
            var version = ++_wardrobePreviewVersion;
            var sprites = await _spriteResolver.Resolve(
                _lastRenderRequest,
                _mainCharacterView,
                _mainCharacterClothes,
                _mainCharacterHair,
                _mainCharacterAccessory);
            if (version == _wardrobePreviewVersion)
                Apply(sprites);
        }

        public UniTask<Sprite> LoadWardrobeThumbnail(
            StoryContracts.StoryChoiceAction actions,
            string value) =>
            _spriteResolver.LoadWardrobeThumbnail(
                actions,
                value,
                _mainCharacterView);

        public async UniTask PreviewWardrobeChoice(
            StoryContracts.StoryChoiceAction actions,
            string value)
        {
            ApplyWardrobeChoice(actions, value);
            var request = _lastRenderRequest;
            if (request == null)
                return;

            var version = ++_wardrobePreviewVersion;
            var sprites = await _spriteResolver.Resolve(
                request,
                _mainCharacterView,
                _mainCharacterClothes,
                _mainCharacterHair,
                _mainCharacterAccessory);
            if (version == _wardrobePreviewVersion)
                Apply(sprites);
        }

        public UniTask Show(
            StoryContracts.StoryCharacterPosition position,
            StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
                return _screen.ShowImage(ToViewPosition(position), _ctx.CancellationToken);
            _screen.ShowImageImmediate(ToViewPosition(position));
            return UniTask.CompletedTask;
        }

        public UniTask Hide(StoryContracts.PresentationMode mode)
        {
            if (mode == StoryContracts.PresentationMode.Animated)
                return _screen.HideImage(_ctx.CancellationToken);
            _screen.HideImageImmediate();
            return UniTask.CompletedTask;
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

        private void ApplyWardrobeChoice(
            StoryContracts.StoryChoiceAction actions,
            string value)
        {
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                SetMainCharacterView(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                SetMainCharacterClothes(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                SetMainCharacterHair(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                SetMainCharacterAccessory(value);
        }

        private void Apply(CharacterSpriteSet sprites)
        {
            var layouts = sprites.TrimLayouts;
            _screen.SetMainBody(sprites.MainBody, layouts.MainBody);
            _screen.SetEmotion(sprites.Emotion, layouts.Emotion);
            _screen.SetClothes(sprites.Clothes, layouts.Clothes);
            _screen.SetBackHairs(sprites.Hair.Back, layouts.BackHair);
            _screen.SetFrontHairs(sprites.Hair.Front, layouts.FrontHair);
            _screen.SetBackAccessories(sprites.Accessories.Back, layouts.BackAccessory);
            _screen.SetMiddleAccessories(sprites.Accessories.Middle, layouts.MiddleAccessory);
            _screen.SetFrontAccessories(sprites.Accessories.Front, layouts.FrontAccessory);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
