using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Notification
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public GameObject NotificationPrefab;
            public CancellationToken CancellationToken;
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
            var screenGO = GameObject.Instantiate(_ctx.NotificationPrefab);
            _screen = screenGO.GetComponent<View.Screen>();
            _screen.HideImmediate();
        }

        public async UniTask Show(string text)
        {
            while(_lastNotifInProcess) await UniTask.NextFrame(_ctx.CancellationToken);
            
            _lastNotifInProcess = true;
            _screen.SetText(text);
            
            await _screen.Show(_ctx.CancellationToken);
            var timer = 3f;
            while(timer > 0)
            {
                await UniTask.Yield(_ctx.CancellationToken);
                timer -= Time.deltaTime;
            }
            await _screen.Hide(_ctx.CancellationToken);
            _lastNotifInProcess = false;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _lastNotifInProcess = false;
            if (_screen != null)
                GameObject.Destroy(_screen.gameObject);
        }
    }
}
