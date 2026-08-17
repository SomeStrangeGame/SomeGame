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
            public string ContentPrefix;
            public Func<string, UniTask<Sprite>> GetSprite;
            public CancellationToken CancellationToken;
        }

        private readonly Ctx _ctx;
        private readonly CharacterSpriteResolver _spriteResolver;
        private View.Screen _screen;
        private string _mainCharacterView;
        private string _mainCharacterClothes;
        private string _mainCharacterHair;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _spriteResolver = new CharacterSpriteResolver(
                ctx.ContentPrefix,
                ctx.GetSprite,
                ctx.CancellationToken);
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
            _spriteResolver.ClearClothes();
        }

        public void SetMainCharacterHair(string hair)
        {
            _mainCharacterHair = hair;
            _spriteResolver.ClearHair();
        }

        public async UniTask SetImage(StoryContracts.CharacterRenderRequest request)
        {
            Apply(await _spriteResolver.Resolve(
                request,
                _mainCharacterView,
                _mainCharacterClothes,
                _mainCharacterHair));
        }

        public UniTask Show(StoryContracts.StoryCharacterPosition position) =>
            _screen.ShowImage(ToViewPosition(position), _ctx.CancellationToken);

        public void ShowImmediate(StoryContracts.StoryCharacterPosition position) =>
            _screen.ShowImageImmediate(ToViewPosition(position));

        public UniTask Hide() => _screen.HideImage(_ctx.CancellationToken);

        public void HideImmediate() => _screen.HideImageImmediate();

        private static bool? ToViewPosition(StoryContracts.StoryCharacterPosition position)
        {
            return position switch
            {
                StoryContracts.StoryCharacterPosition.Left => true,
                StoryContracts.StoryCharacterPosition.Right => false,
                _ => null,
            };
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

        protected override void OnDispose()
        {
            base.OnDispose();
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
