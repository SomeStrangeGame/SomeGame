using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Waiting.Entity CreateWaiting()
        {
            return new Waiting.Entity().AddTo(this);
        }
    }
}

