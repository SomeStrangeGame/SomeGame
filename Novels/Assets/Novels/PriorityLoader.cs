using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels
{
    internal sealed class PriorityLoader
    {
        private readonly ThreadPriority _defaultPriority;

        internal PriorityLoader(ThreadPriority defaultPriority)
        {
            _defaultPriority = defaultPriority;
        }

        internal async UniTask Run(Func<UniTask> operation)
        {
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultPriority))
                await operation();
        }

        internal async UniTask<T> Run<T>(Func<UniTask<T>> operation)
        {
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultPriority))
                return await operation();
        }
    }
}
