using System;
using UnityEngine;

namespace LoadingPriority
{
    public class Entity : IDisposable
    {
        private readonly ThreadPriority _defaultPriority;

        public Entity(ThreadPriority currentPriority, ThreadPriority defaultPriority)
        {
            _defaultPriority = defaultPriority;
            Application.backgroundLoadingPriority = currentPriority;
        }

        public void Dispose()
        {
            Application.backgroundLoadingPriority = _defaultPriority;
        }
    }
}
