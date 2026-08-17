using System.Threading;
using System.Collections.Generic;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Bundles.Entity CreateBundles()
        {
            var bundles = new Bundles.Entity(new Bundles.Entity.Ctx
            {
                Prefix = _definition.Prefix,
                CancellationToken = _ctx.CancellationToken,
                OnLog = _ctx.OnLog,
            }).AddTo(this);
            bundles.ConfigureMedia(new Bundles.MediaManifest(
                _definition.Episode.Media.VideoIds,
                new Dictionary<string, string>
                {
                    ["Horror"] = ".mp3",
                    ["основная"] = ".WAV",
                }));
            return bundles;
        }
    }
}
