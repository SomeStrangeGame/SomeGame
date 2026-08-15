using System;
using System.Collections.Generic;

namespace Disposable 
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
        private bool _isDisposed;

        public bool IsDisposed => _isDisposed;

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            Exception firstException = null;

            while (_disposables.Count > 0)
            {
                try
                {
                    _disposables.Pop().Dispose();
                }
                catch (Exception exception)
                {
                    firstException ??= exception;
                }
            }

            try
            {
                OnDispose();
            }
            catch (Exception exception)
            {
                firstException ??= exception;
            }

            if (firstException != null)
                throw firstException;
        }

        public void AddDisposable(IDisposable disposable)
        {
            if (disposable == null)
                throw new ArgumentNullException(nameof(disposable));

            if (_isDisposed)
            {
                disposable.Dispose();
                return;
            }

            _disposables.Push(disposable);
        }

        protected virtual void OnDispose() { }
    }
}
