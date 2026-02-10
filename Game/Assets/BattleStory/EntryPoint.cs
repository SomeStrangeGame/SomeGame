using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BattleStory
{
    internal sealed class EntryPoint : MonoBehaviour
    {
        [Flags]
        private enum ShowLogs : byte
        {
            Error = 1 << 0,
            Assert = 1 << 1,
            Warning = 1 << 2,
            Log = 1 << 3,
            Exception = 1 << 4,
        }

        [SerializeField] private Data _data;
        [SerializeField] private ShowLogs _logs;

        private Entity _entity;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            Application.targetFrameRate = 30;

            _entity = new Entity(new Entity.Ctx
            {
                Data = _data,
                OnLog = Log,
            });
            _entity.Init().Forget();
        }

        private void OnDisable()
        {
            _entity?.Dispose();
        }

        private void Log((LogType type, string message) log)
        {
            var color = Color.white;
            var isShowLog = false;
            switch (log.type)
            {
                case LogType.Error:
                    isShowLog |= _logs.HasFlag(ShowLogs.Error);
                    color = Color.red;
                    break;
                case LogType.Assert:
                    isShowLog |= _logs.HasFlag(ShowLogs.Assert);
                    color = Color.red;
                    break;
                case LogType.Warning:
                    isShowLog |= _logs.HasFlag(ShowLogs.Warning);
                    color = Color.yellow;
                    break;
                case LogType.Log:
                    isShowLog |= _logs.HasFlag(ShowLogs.Log);
                    color = Color.white;
                    break;
                case LogType.Exception:
                    isShowLog |= _logs.HasFlag(ShowLogs.Exception);
                    color = Color.red;
                    break;
            }
            if (!isShowLog) return;

            Debug.Log($"[BattleStory] <color=#{ColorUtility.ToHtmlStringRGB(color)}>{log.message}</color>");
        }
    }
}