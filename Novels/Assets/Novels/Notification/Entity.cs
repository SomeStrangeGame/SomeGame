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
            public Func<UniTask<GameObject>> GetNotificationPrefab;
        }

        private bool _lastNotifInProcess;

        private readonly Ctx _ctx;

        private View.Screen _screen;

        public Entity(Ctx ctx)
        {
            _lastNotifInProcess = false;
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            var prefab = await _ctx.GetNotificationPrefab();
            var screenGO = GameObject.Instantiate(prefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public async UniTask Show(string text)
        {
            while(_lastNotifInProcess) await UniTask.NextFrame();
            
            _lastNotifInProcess = true;
            _screen.SetText(text);
            await _screen.Show();
            await UniTask.Delay(3000);
            await _screen.Hide();
            _lastNotifInProcess = false;
        }
    }
}

