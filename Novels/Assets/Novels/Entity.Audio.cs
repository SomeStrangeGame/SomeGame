using System;
using System.Threading;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {   
        private Audio.Entity CreateAudio(Func<string, string> getAudioURL, Action<string> loadAudionToDict)
        {
            return new Audio.Entity(new Audio.Entity.Ctx
            {
                GetAudioURL = getAudioURL,
                LoadAudioToDict = loadAudionToDict,
                AudioMixer = _ctx.Data.AudioMixer,
                CancellationToken = _ctx.CancellationToken,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
        }
    }
}
