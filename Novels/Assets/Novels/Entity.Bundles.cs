using System.Threading;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Bundles.Entity CreateBundles()
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                Prefix = _ctx.Data.Prefix,
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
            }).AddTo(this);
        }
    }
}
