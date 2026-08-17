using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private PathGetter.Entity CreatePathGetter()
        {
            return new PathGetter.Entity(new PathGetter.Entity.Ctx
            {
                Prefix = _definition.Prefix,
                EpisodeId = _episode.Id,
            }).AddTo(this);
        }
    }
}
