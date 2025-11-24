using System;
using System.Collections.Generic;

namespace Game.Disposable 
{
    public static class BaseDisposableEx 
    {
        public static T AddTo<T>(this T disposable, IBaseDisposable owner) where T : IDisposable
        {
            owner.AddDisposable(disposable);
            return disposable;
        }
    }

    public interface IBaseDisposable : IDisposable
    {
        public void AddDisposable(IDisposable disposable);
    }

    public abstract class BaseDisposable : IBaseDisposable
    {
        private readonly Stack<IDisposable> _disposables = new ();
        public bool IsDisposed = false;

        public void Dispose()
        {
            while (_disposables.Count > 0)
                _disposables.Pop().Dispose();
            IsDisposed = true;
            OnDispose();
        }

        public void AddDisposable(IDisposable disposable) => _disposables.Push(disposable);

        protected virtual void OnDispose() { }
    }
}
