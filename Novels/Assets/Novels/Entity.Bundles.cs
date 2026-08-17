using System.Threading;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Bundles.Entity CreateBundles()
        {
            var bundles = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
                OnFailure = failure => _ctx.OnError?.Invoke(
                    new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.BundleFailure,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        $"[{failure.Code}] {failure.Message}",
                        exception: failure.Exception)),
            }).AddTo(this);
            return bundles;
        }

        private void ConfigureMedia(Bundles.Entity bundles)
        {
            bundles.ConfigureMedia(
                _definition.Prefix,
                new Bundles.MediaManifest(
                    _definition.Episode.Media.VideoIds,
                    _definition.Episode.Media.AudioExtensions,
                    _definition.Episode.Media.DefaultAudioExtension,
                    _definition.Episode.Media.SilentAudioIds));
        }
    }
}
