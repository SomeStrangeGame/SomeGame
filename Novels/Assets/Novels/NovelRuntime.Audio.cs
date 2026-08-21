using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class NovelRuntime
    {   
        private Audio.AudioController CreateAudio(
            IBaseDisposable owner,
            Func<string, UniTask<string>> resolveAudioUrl,
            CancellationToken cancellationToken)
        {
            return new Audio.AudioController(new Audio.AudioController.Dependencies
            {
                ResolveAudioUrl = resolveAudioUrl,
                AudioMixer = _audioMixer,
                CancellationToken = cancellationToken,

                OnLog = _ctx.OnLog,
                OnError = ReportError,
            }).AddTo(owner);
        }
    }
}
