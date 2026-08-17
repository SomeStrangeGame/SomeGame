using System;
using System.Collections.Generic;
using UnityEngine;

namespace Novels.Audio
{
    internal sealed class LoopAudioChannels : IDisposable
    {
        private readonly AudioMixerGroups _mixerGroups;
        private readonly Dictionary<Entity.Audio, AudioSource> _sources = new();

        internal LoopAudioChannels(AudioMixerGroups mixerGroups)
        {
            _mixerGroups = mixerGroups;
        }

        internal void Play(string assetName, AudioClip clip, Entity.Audio channel)
        {
            if (channel == Entity.Audio.Sound)
                throw new ArgumentOutOfRangeException(nameof(channel));
            var source = GetSource(channel);
            source.Stop();
            DestroyClip(source.clip);
            source.name = $"{assetName}-{channel}";
            source.clip = clip;
            source.loop = true;
            source.outputAudioMixerGroup = _mixerGroups.Get(channel);
            source.Play();
        }

        internal void Clear(Entity.Audio channel)
        {
            if (!_sources.TryGetValue(channel, out var source))
                return;
            source.Stop();
            var clip = source.clip;
            source.clip = null;
            DestroyClip(clip);
        }

        public void Dispose()
        {
            foreach (var source in _sources.Values)
            {
                if (source == null)
                    continue;
                DestroyClip(source.clip);
                GameObject.Destroy(source.gameObject);
            }
            _sources.Clear();
        }

        private AudioSource GetSource(Entity.Audio channel)
        {
            if (_sources.TryGetValue(channel, out var source))
                return source;
            var audioObject = new GameObject($"NovelAudio-{channel}");
            source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sources[channel] = source;
            return source;
        }

        private static void DestroyClip(AudioClip clip)
        {
            if (clip != null)
                GameObject.Destroy(clip);
        }
    }
}
