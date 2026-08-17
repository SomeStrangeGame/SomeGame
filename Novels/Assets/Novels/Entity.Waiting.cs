using Disposable;
using System.Threading;

namespace Novels
{
    internal partial class Entity
    {
        private Waiting.Entity CreateWaiting(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            return new Waiting.Entity(new Waiting.Entity.Ctx
            {
                CancellationToken = cancellationToken,
            }).AddTo(owner);
        }
    }
}
