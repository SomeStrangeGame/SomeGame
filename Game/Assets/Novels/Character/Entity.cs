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
            public string MainCharacterName;
            public Func<UniTask<GameObject>> GetScreenPrefab;
            public Func<string, UniTask<Sprite>> GetSprite;
            public Func<string, string, string, string> GetMainBodyPath;
            public Func<string, string, string, string> GetEmotionPath;
            public Func<string, string, int, string> GetWeatherPath;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _wardrobe = "Wardrobe";

        private readonly Ctx _ctx;

        private View.Screen _screen;

        private string _mainCharacterView;
        private string _mainCharacterWeather;


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
            else if (view == "Афроамериканская")
                view = "Afro";
            else if (view == "Латиноамериканская")
                view = "Latin";
            _mainCharacterView = $"View/{view}";
        }

        public void SetMainCharacterWeather(string weather)
        {
            if (weather == "Повседневная одежда")
                weather = "Casual wear";
            else if (weather == "Летний сарафан")
                weather = "Summer sundress";
            else if (weather == "Модный топ")
                weather = "Fashionable top";
                
            _mainCharacterWeather = weather;
        }

        public async UniTask SetImageAndShow(string name, params string[] args)
        {
            var view = "View";
            var weather = string.Empty;
            if (name == _ctx.MainCharacterName || name == _wardrobe)
            {
                name = _mainCharacter;
                view = _mainCharacterView;
                weather = _mainCharacterWeather;
            }

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

            var defaultWeatherSprite = await _ctx.GetSprite(_ctx.GetWeatherPath(name, weather, 1));
            _screen.SetWeather(defaultWeatherSprite);
            foreach (var arg in args)
            {
                var weatherSprite = await _ctx.GetSprite(_ctx.GetWeatherPath(name, arg, 1));
                if (weatherSprite != null) 
                {
                    _screen.SetWeather(weatherSprite);
                    break;
                }
            }

            await _screen.ShowImage();
        }

        public async UniTask Hide()
        {
            await _screen.HideImage();
        }
    }
}

