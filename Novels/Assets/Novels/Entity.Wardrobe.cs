using System.Threading;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private static Wardrobe.Entity CreateWardrobe(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            var wardrobe = new Wardrobe.Entity(new Wardrobe.Entity.Ctx
            {
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            wardrobe.Init();
            return wardrobe;
        }
    }
}
