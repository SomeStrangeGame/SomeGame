using System;

namespace Disposable
{
    public sealed class DisposableSlot<T> : BaseDisposable
        where T : class, IDisposable
    {
        public T Value { get; private set; }

        public void Replace(T value)
        {
            if (ReferenceEquals(Value, value))
                return;
            var previous = Value;
            Value = value;
            previous?.Dispose();
        }

        public void Clear(T expected = null)
        {
            if (expected != null && !ReferenceEquals(Value, expected))
                return;
            Replace(null);
        }

        protected override void OnDispose()
        {
            Clear();
            base.OnDispose();
        }
    }
}
