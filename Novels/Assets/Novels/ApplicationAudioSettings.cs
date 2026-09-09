using Disposable;
using UnityEngine;

namespace Novels
{
    internal sealed class ApplicationAudioSettings : BaseDisposable, Catalog.ICatalogSettings
    {
        private const string VolumeKey = "Novels.Settings.MasterVolume.v1";
        private readonly float _previousVolume = AudioListener.volume;
        private bool _dirty;

        internal ApplicationAudioSettings()
        {
            var saved = PlayerPrefs.GetFloat(VolumeKey, 1f);
            Volume = float.IsNaN(saved) || float.IsInfinity(saved) ? 1f : Mathf.Clamp01(saved);
        }

        public float Volume { get; private set; }

        internal void Apply() => AudioListener.volume = Volume;

        public void SetVolume(float volume)
        {
            if (float.IsNaN(volume) || float.IsInfinity(volume)) return;
            Volume = Mathf.Clamp01(volume);
            Apply();
            PlayerPrefs.SetFloat(VolumeKey, Volume);
            _dirty = true;
        }

        public void Save()
        {
            if (!_dirty) return;
            PlayerPrefs.Save();
            _dirty = false;
        }

        protected override void OnDispose()
        {
            Save();
            AudioListener.volume = _previousVolume;
            base.OnDispose();
        }
    }
}
