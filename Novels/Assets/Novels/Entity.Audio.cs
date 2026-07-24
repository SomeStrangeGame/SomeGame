using Disposable;

namespace Novels
{
    internal partial class Entity
    {   
        private Audio.Entity CreateAudio(Bundles.Entity bundles)
        {
            return new Audio.Entity(new Audio.Entity.Ctx
            {
                GetAudioURL = assetName => bundles.GetAudioURL(assetName),
                LoadAudioToDict = bundles.LoadAudioToDict,
                AudioMixer = _ctx.Data.AudioMixer,

                OnLog = _ctx.OnLog,
            }).AddTo(this);
        }
    }
}

