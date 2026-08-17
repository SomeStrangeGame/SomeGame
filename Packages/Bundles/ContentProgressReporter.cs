using System;
using System.Threading;
using UnityEngine;

namespace Bundles
{
    internal sealed class ContentProgressReporter<T>
    {
        private readonly Action<T> _observer;
        private readonly Action<(LogType type, string message)> _onLog;
        private int _disabled;

        internal ContentProgressReporter(
            Action<T> observer,
            Action<(LogType type, string message)> onLog)
        {
            _observer = observer;
            _onLog = onLog;
        }

        internal void Report(T value)
        {
            if (_observer == null || Volatile.Read(ref _disabled) != 0)
                return;
            try
            {
                _observer(value);
            }
            catch (Exception exception)
            {
                if (Interlocked.Exchange(ref _disabled, 1) != 0)
                    return;
                try
                {
                    _onLog?.Invoke((
                        LogType.Warning,
                        $"Content progress observer failed: {exception.Message}"));
                }
                catch
                {
                }
            }
        }
    }
}
