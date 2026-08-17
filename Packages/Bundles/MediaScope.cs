using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bundles
{
    public sealed class MediaScope : Scope
    {
        private readonly MediaResolver _media;

        internal MediaScope(
            Entity owner,
            ContentReleaseSession session,
            CancellationToken cancellationToken,
            MediaResolver media)
            : base(owner, session, cancellationToken)
        {
            _media = media ?? throw new ArgumentNullException(nameof(media));
        }

        public UniTask<string> ResolveVideoUrl(string assetName)
        {
            EnsureActive();
            return _media.ResolveVideoUrl(assetName);
        }

        public UniTask<string> ResolveAudioUrl(string assetName)
        {
            EnsureActive();
            return _media.ResolveAudioUrl(assetName);
        }
    }
}
