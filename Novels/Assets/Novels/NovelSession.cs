using Bundles;
using Disposable;

namespace Novels
{
    internal sealed class NovelSession : BaseDisposable
    {
        internal NovelSession(Scope bundles)
        {
            Bundles = bundles;
            Bundles.AddTo(this);
        }

        internal Scope Bundles { get; }

        internal void AttachDelivery(ContentDeliveryLease lease)
        {
            lease?.AddTo(this);
        }
    }
}
