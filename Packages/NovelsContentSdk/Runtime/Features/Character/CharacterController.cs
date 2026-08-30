using System;
using System.Collections.Generic;
using System.Linq;
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
            public Func<string, string> ResolveArtAddress;
            public Content.CharacterAssetProfile AssetProfile;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, UniTask<Sprite>> GetFullQualitySprite;
            public Func<UniTask<CharacterSpriteTrimManifest>> GetSpriteTrimManifest;
            public Sprite MissingCharacter;
            public Action<string, string> OnFallback;
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
        private bool _isVisible;
        private StoryContracts.StoryCharacterPosition _visiblePosition;
        private bool _wardrobeRestoreVisible;
        private StoryContracts.StoryCharacterPosition _wardrobeRestorePosition;
        private StoryContracts.CharacterRenderRequest _wardrobeRestoreRequest;
        private string _wardrobeRestoreTarget;
        private int _wardrobePreviewVersion;
        private string _wardrobeTarget;
        private readonly Dictionary<string, WardrobeLook> _characterWardrobe =
            new(StringComparer.OrdinalIgnoreCase);

        private sealed class WardrobeLook
        {
            internal string Clothes;
            internal string Hair;
            internal string Accessory;
        }

        public CharacterController(Dependencies ctx)
        {
            _ctx = ctx;
            _assetProfile = ctx.AssetProfile
                ?? throw new ArgumentNullException(nameof(ctx.AssetProfile));
            _mainCharacterView = _assetProfile.ViewRoot;
            _spriteResolver = new CharacterSpriteResolver(
                ctx.ContentPrefix,
                ctx.ResolveArtAddress,
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
                _mainCharacterAccessory,
                _wardrobeTarget,
                TargetLook.Clothes,
                TargetLook.Hair,
                TargetLook.Accessory), request);
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
                _mainCharacterAccessory,
                _wardrobeTarget,
                TargetLook.Clothes,
                TargetLook.Hair,
                TargetLook.Accessory);
            if (version == _wardrobePreviewVersion)
                Apply(sprites, _lastRenderRequest);
        }

        public UniTask<Sprite> LoadWardrobeThumbnail(
            StoryContracts.StoryChoiceAction actions,
            string value) =>
            _spriteResolver.LoadWardrobeThumbnail(
                actions,
                value,
                _mainCharacterView,
                _wardrobeTarget);

        public void SetWardrobeTarget(string character)
        {
            _wardrobeTarget = character?.Trim() ?? string.Empty;
        }

        public void ApplyWardrobeSelection(
            string character,
            StoryContracts.StoryChoiceAction actions,
            string value)
        {
            SetWardrobeTarget(character);
            ApplyWardrobeChoice(actions, value);
        }

        public string GetCurrentWardrobeValue(
            StoryContracts.StoryChoiceAction actions)
        {
            if (!string.IsNullOrWhiteSpace(_wardrobeTarget))
            {
                var look = TargetLook;
                var defaults = _assetProfile.Defaults(_wardrobeTarget);
                if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                    return look.Clothes ?? defaults.Clothes;
                if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                    return look.Hair ?? defaults.Hair;
                if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                    return look.Accessory ?? defaults.Accessory;
                return string.Empty;
            }

            var mainDefaults = _assetProfile.Defaults(
                _assetProfile.MainCharacterAssetId);
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
            {
                var prefix = $"{_assetProfile.ViewRoot}/";
                return _mainCharacterView?.StartsWith(
                        prefix,
                        StringComparison.OrdinalIgnoreCase) == true
                    ? _mainCharacterView.Substring(prefix.Length)
                    : string.Empty;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                return _mainCharacterClothes ?? mainDefaults.Clothes;
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                return _mainCharacterHair ?? mainDefaults.Hair;
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                return _mainCharacterAccessory ?? mainDefaults.Accessory;
            return string.Empty;
        }

        public async UniTask<string[]> LoadWardrobeCategory(
            StoryContracts.StoryChoiceAction actions)
        {
            var manifest = await _ctx.GetSpriteTrimManifest();
            if (manifest == null)
                return Array.Empty<string>();
            var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in manifest.Entries)
            {
                if (TryGetWardrobeValue(entry.AssetAddress, actions, out var value))
                    values.Add(value);
            }
            return values.OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }

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
                _mainCharacterAccessory,
                _wardrobeTarget,
                TargetLook.Clothes,
                TargetLook.Hair,
                TargetLook.Accessory);
            if (version == _wardrobePreviewVersion)
                Apply(sprites, request);
        }

        public UniTask Show(
            StoryContracts.StoryCharacterPosition position,
            StoryContracts.PresentationMode mode)
        {
            _isVisible = true;
            _visiblePosition = position;
            if (mode == StoryContracts.PresentationMode.Animated)
                return _screen.ShowImage(ToViewPosition(position), _ctx.CancellationToken);
            _screen.ShowImageImmediate(ToViewPosition(position));
            return UniTask.CompletedTask;
        }

        public UniTask Hide(StoryContracts.PresentationMode mode)
        {
            _isVisible = false;
            if (mode == StoryContracts.PresentationMode.Animated)
                return _screen.HideImage(_ctx.CancellationToken);
            _screen.HideImageImmediate();
            return UniTask.CompletedTask;
        }

        public async UniTask BeginWardrobePreview(string character)
        {
            _wardrobeRestoreVisible = _isVisible;
            _wardrobeRestorePosition = _visiblePosition;
            _wardrobeRestoreRequest = _lastRenderRequest;
            _wardrobeRestoreTarget = _wardrobeTarget;
            SetWardrobeTarget(character);
            await SetImage(new StoryContracts.CharacterRenderRequest(
                character,
                StoryContracts.StorySpeakerRole.Wardrobe,
                StoryContracts.StoryCharacterPosition.Center,
                new StoryContracts.CharacterPresentation(
                    false,
                    false,
                    false,
                    false,
                    string.Empty,
                    null,
                    StoryContracts.StoryCharacterVisibilityCommand.Unchanged,
                    false,
                    Array.Empty<string>())));
            await Show(
                StoryContracts.StoryCharacterPosition.Center,
                StoryContracts.PresentationMode.Immediate);
        }

        public async UniTask EndWardrobePreview()
        {
            var restoreRequest = _wardrobeRestoreRequest;
            SetWardrobeTarget(_wardrobeRestoreTarget);
            if (restoreRequest != null)
                await SetImage(restoreRequest);
            if (_wardrobeRestoreVisible)
            {
                await Show(
                    _wardrobeRestorePosition,
                    StoryContracts.PresentationMode.Immediate);
            }
            else
            {
                await Hide(StoryContracts.PresentationMode.Immediate);
            }
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
            if (!string.IsNullOrWhiteSpace(_wardrobeTarget))
            {
                var look = TargetLook;
                if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                    look.Clothes = value;
                if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                    look.Hair = value;
                if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                    look.Accessory = value;
                return;
            }
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
                SetMainCharacterView(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
                SetMainCharacterClothes(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
                SetMainCharacterHair(value);
            if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
                SetMainCharacterAccessory(value);
        }

        private WardrobeLook TargetLook
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_wardrobeTarget))
                    return new WardrobeLook();
                if (_characterWardrobe.TryGetValue(_wardrobeTarget, out var look))
                    return look;
                look = new WardrobeLook();
                _characterWardrobe.Add(_wardrobeTarget, look);
                return look;
            }
        }

        private static bool TryGetWardrobeValue(
            string address,
            StoryContracts.StoryChoiceAction actions,
            out string value)
        {
            value = string.Empty;
            if (string.IsNullOrWhiteSpace(address))
                return false;
            const string marker = "/maincharacter/";
            var normalized = address.Replace('\\', '/');
            var markerIndex = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0)
                return false;
            var parts = normalized.Substring(markerIndex + marker.Length).Split('/');
            if ((actions & StoryContracts.StoryChoiceAction.SelectAppearance) != 0)
            {
                if (parts.Length == 3
                    && string.Equals(parts[0], "view", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(parts[2], "main.png", StringComparison.OrdinalIgnoreCase))
                    value = parts[1];
            }
            else if ((actions & StoryContracts.StoryChoiceAction.SelectHair) != 0)
            {
                if (parts.Length == 4
                    && string.Equals(parts[0], "hairs", StringComparison.OrdinalIgnoreCase))
                    value = parts[2];
            }
            else if ((actions & StoryContracts.StoryChoiceAction.SelectClothes) != 0)
            {
                if (parts.Length == 3
                    && string.Equals(parts[0], "clothes", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(parts[2], "1.png", StringComparison.OrdinalIgnoreCase))
                    value = parts[1];
            }
            else if ((actions & StoryContracts.StoryChoiceAction.SelectAccessory) != 0)
            {
                if (parts.Length == 3
                    && string.Equals(parts[0], "accessories", StringComparison.OrdinalIgnoreCase)
                    && parts[2].EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    value = parts[2].Substring(0, parts[2].Length - 4);
                }
            }
            return !string.IsNullOrWhiteSpace(value);
        }

        private void Apply(
            CharacterSpriteSet sprites,
            StoryContracts.CharacterRenderRequest request)
        {
            if (ReferenceEquals(sprites.MainBody, _ctx.MissingCharacter))
            {
                _ctx.OnFallback?.Invoke(
                    request?.Name ?? string.Empty,
                    "required_character_assets_missing");
            }
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
