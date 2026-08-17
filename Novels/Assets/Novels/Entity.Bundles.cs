namespace Novels
{
    internal partial class Entity
    {
        private void ConfigureMedia(Bundles.Scope bundles)
        {
            bundles.ConfigureMedia(
                _definition.Prefix,
                new Bundles.MediaManifest(
                    _episode.Media.VideoIds,
                    _episode.Media.AudioExtensions,
                    _episode.Media.DefaultAudioExtension,
                    _episode.Media.SilentAudioIds));
        }
    }
}
