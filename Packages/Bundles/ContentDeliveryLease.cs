using System;
using System.Threading;

namespace Bundles
{
    public sealed class ContentDeliveryLease : IDisposable
    {
        private Action _release;

        internal ContentDeliveryLease(Action release)
        {
            _release = release ?? throw new ArgumentNullException(nameof(release));
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _release, null)?.Invoke();
        }
    }
}
