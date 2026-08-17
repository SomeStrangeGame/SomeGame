using System;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Novels.Audio
{
    internal sealed class AudioMixerGroups
    {
        private static readonly IReadOnlyDictionary<Entity.Audio, string> _names =
            new Dictionary<Entity.Audio, string>
            {
                [Entity.Audio.Music] = "Music",
                [Entity.Audio.Sound] = "Sound",
                [Entity.Audio.Ambient] = "Ambient",
            };

        private readonly AudioMixer _mixer;
        private readonly Dictionary<Entity.Audio, AudioMixerGroup> _groups = new();

        internal AudioMixerGroups(AudioMixer mixer)
        {
            _mixer = mixer;
        }

        internal AudioMixerGroup Get(Entity.Audio channel)
        {
            if (_groups.TryGetValue(channel, out var group))
                return group;
            if (_mixer == null)
                throw new InvalidOperationException("AudioMixer is not configured.");
            var groupName = _names[channel];
            var matches = _mixer.FindMatchingGroups(groupName);
            if (matches.Length == 0)
            {
                throw new InvalidOperationException(
                    $"AudioMixer group '{groupName}' is not configured.");
            }
            group = matches[0];
            _groups[channel] = group;
            return group;
        }
    }
}
