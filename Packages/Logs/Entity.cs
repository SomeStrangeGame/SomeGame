using System;
using Disposable;
using UnityEngine;

namespace Logs
{
    public class Entity : BaseDisposable
    {
        [Flags]
        public enum ShowLogs : byte
        {
            Error = 1 << 0,
            Assert = 1 << 1,
            Warning = 1 << 2,
            Log = 1 << 3,
            Exception = 1 << 4,
        }

        public struct Ctx
        {
            public ShowLogs Logs;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Log(string prefix, (LogType type, string message) log)
        {
            var color = Color.white;
            var isShowLog = false;
            switch (log.type)
            {
                case LogType.Error:
                    isShowLog |= _ctx.Logs.HasFlag(ShowLogs.Error);
                    color = Color.red;
                    break;
                case LogType.Assert:
                    isShowLog |= _ctx.Logs.HasFlag(ShowLogs.Assert);
                    color = Color.red;
                    break;
                case LogType.Warning:
                    isShowLog |= _ctx.Logs.HasFlag(ShowLogs.Warning);
                    color = Color.yellow;
                    break;
                case LogType.Log:
                    isShowLog |= _ctx.Logs.HasFlag(ShowLogs.Log);
                    color = Color.white;
                    break;
                case LogType.Exception:
                    isShowLog |= _ctx.Logs.HasFlag(ShowLogs.Exception);
                    color = Color.red;
                    break;
            }
            if (!isShowLog) return;

            Debug.Log($"{prefix} <color=#{ColorUtility.ToHtmlStringRGB(color)}>{log.message}</color>");
        }
    }
}