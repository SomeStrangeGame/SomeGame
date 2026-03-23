using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Notification
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<GameObject> GetNotificationPrefab;
        }

        private bool _lastNotifInProcess;

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _lastNotifInProcess = false;
            _ctx = ctx;
        }

        public void Init()
        {
            var prefab = _ctx.GetNotificationPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public async UniTask Show(bool isLoading, string text)
        {
            while(_lastNotifInProcess) await UniTask.NextFrame();
            
            _lastNotifInProcess = true;
            _screen.SetText(text);
            if (isLoading)
                _screen.ShowImmediate();
            else
                await _screen.Show();
            var timer = isLoading ? 0f : 3f;
            while(timer > 0)
            {
                await UniTask.Yield();
                timer -= Time.deltaTime;
            }
            if (isLoading)
                _screen.HideImmediate();
            else
                await _screen.Hide();
            _lastNotifInProcess = false;
        }
    }
}

