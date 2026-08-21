using System.Threading;
using Disposable;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private static Wardrobe.WardrobeController CreateWardrobe(
            IBaseDisposable owner,
            CancellationToken cancellationToken)
        {
            var wardrobe = new Wardrobe.WardrobeController(new Wardrobe.WardrobeController.Dependencies
            {
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            wardrobe.Init();
            return wardrobe;
        }
    }
}
