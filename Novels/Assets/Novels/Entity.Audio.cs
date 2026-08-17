using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {   
        private Audio.Entity CreateAudio(
            IBaseDisposable owner,
            Func<string, UniTask<string>> resolveAudioUrl)
        {
            return new Audio.Entity(new Audio.Entity.Ctx
            {
                ResolveAudioUrl = resolveAudioUrl,
                AudioMixer = _ctx.Data.AudioMixer,
                CancellationToken = _ctx.CancellationToken,

                OnLog = _ctx.OnLog,
                OnError = _ctx.OnError,
            }).AddTo(owner);
        }
    }
}
