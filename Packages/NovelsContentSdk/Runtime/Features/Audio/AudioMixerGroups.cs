using System;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Novels.Audio
{
    internal sealed class AudioMixerGroups
    {
        private static readonly IReadOnlyDictionary<AudioController.Audio, string> _names =
            new Dictionary<AudioController.Audio, string>
            {
                [AudioController.Audio.Music] = "Music",
                [AudioController.Audio.Sound] = "Sound",
                [AudioController.Audio.Ambient] = "Ambient",
            };

        private readonly AudioMixer _mixer;
        private readonly Dictionary<AudioController.Audio, AudioMixerGroup> _groups = new();

        internal AudioMixerGroups(AudioMixer mixer)
        {
            _mixer = mixer;
        }

        internal AudioMixerGroup Get(AudioController.Audio channel)
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
