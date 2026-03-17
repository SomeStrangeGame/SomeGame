using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Bundles.Entity CreateBundles()
        {
            return new Bundles.Entity(new Bundles.Entity.Ctx
            {
                OnLog = _ctx.OnLog,
            }).AddTo(this);
        }
    }
}

