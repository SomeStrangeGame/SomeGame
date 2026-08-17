using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Waiting.Entity CreateWaiting(IBaseDisposable owner)
        {
            return new Waiting.Entity(new Waiting.Entity.Ctx
            {
                CancellationToken = _ctx.CancellationToken,
            }).AddTo(owner);
        }
    }
}
