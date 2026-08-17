namespace Novels
{
    internal partial class Entity
    {
        private EpisodeRuntime CreateEpisodeRuntime()
        {
            return new EpisodeRuntime(_ctx.CancellationToken);
        }
    }
}
