using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.Audio
{
    internal sealed class SoundVoicePool : IDisposable
    {
        private readonly int _voiceCount;
        private readonly int _cacheCapacity;
        private readonly AudioMixerGroups _mixerGroups;
        private readonly List<AudioSource> _voices = new();
        private readonly Dictionary<string, AudioClip> _clips = new(
            StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _clipOrder = new();

        internal SoundVoicePool(
            int voiceCount,
            int cacheCapacity,
            AudioMixerGroups mixerGroups)
        {
            _voiceCount = voiceCount;
            _cacheCapacity = cacheCapacity;
            _mixerGroups = mixerGroups;
        }

        internal bool TryGetClip(string assetName, out AudioClip clip) =>
            _clips.TryGetValue(assetName, out clip);

        internal void Play(string assetName, AudioClip clip)
        {
            Cache(assetName, clip);
            var voice = GetVoice();
            voice.Stop();
            voice.name = $"{assetName}-{Entity.Audio.Sound}";
            voice.clip = clip;
            voice.loop = false;
            voice.outputAudioMixerGroup = _mixerGroups.Get(Entity.Audio.Sound);
            voice.Play();
        }

        internal void Clear()
        {
            foreach (var voice in _voices)
            {
                voice.Stop();
                voice.clip = null;
            }
        }

        public void Dispose()
        {
            foreach (var voice in _voices)
            {
                if (voice != null)
                    GameObject.Destroy(voice.gameObject);
            }
            _voices.Clear();
            foreach (var clip in _clips.Values)
            {
                if (clip != null)
                    GameObject.Destroy(clip);
            }
            _clips.Clear();
            _clipOrder.Clear();
        }

        private AudioSource GetVoice()
        {
            foreach (var voice in _voices)
            {
                if (!voice.isPlaying)
                    return voice;
            }
            if (_voices.Count < _voiceCount)
            {
                var audioObject = new GameObject($"NovelAudio-Sound-{_voices.Count}");
                var voice = audioObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                _voices.Add(voice);
                return voice;
            }
            return _voices[0];
        }

        private void Cache(string assetName, AudioClip clip)
        {
            if (_clips.ContainsKey(assetName))
                return;
            _clips[assetName] = clip;
            _clipOrder.Enqueue(assetName);
            var attempts = _clipOrder.Count;
            while (_clips.Count > _cacheCapacity && attempts-- > 0)
            {
                var oldest = _clipOrder.Dequeue();
                if (!_clips.TryGetValue(oldest, out var removed))
                    continue;
                if (IsInUse(removed))
                {
                    _clipOrder.Enqueue(oldest);
                    continue;
                }
                _clips.Remove(oldest);
                if (removed != null)
                    GameObject.Destroy(removed);
            }
        }

        private bool IsInUse(AudioClip clip)
        {
            foreach (var voice in _voices)
            {
                if (voice.isPlaying && voice.clip == clip)
                    return true;
            }
            return false;
        }
    }
}
