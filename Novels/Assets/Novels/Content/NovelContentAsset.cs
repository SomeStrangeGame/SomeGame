using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Novels.Content
{
    public static class ContentLocalizationKeys
    {
        public static string EpisodeTitle(string episodeId)
        {
            if (string.IsNullOrWhiteSpace(episodeId))
                throw new ArgumentException("Episode ID must not be empty.", nameof(episodeId));
            return $"episode.{episodeId.Trim()}.title";
        }
    }

    [CreateAssetMenu(fileName = "NovelContent", menuName = "Novels/Content")]
    public sealed class NovelContentAsset : ScriptableObject
    {
        [Serializable]
        private struct AudioExtensionEntry
        {
            [SerializeField] private string _assetId;
            [SerializeField] private string _extension;

            internal readonly string AssetId => _assetId;
            internal readonly string Extension => _extension;
        }

        [Serializable]
        private struct EpisodeEntry
        {
            [SerializeField] private string _id;
            [SerializeField] private string _titleKey;
            [SerializeField] private string _storyPath;
            [SerializeField] private string _contentVersion;
            [SerializeField] private string[] _videoIds;
            [SerializeField] private string _defaultAudioExtension;
            [SerializeField] private string[] _silentAudioIds;
            [SerializeField] private string[] _audioDependencies;
            [SerializeField] private string[] _backgroundDependencies;
            [SerializeField] private string[] _speakerDependencies;
            [SerializeField] private AudioExtensionEntry[] _audioExtensions;

            internal readonly EpisodeDefinition ToDefinition(
                string contentId,
                Func<string, string> getRequiredLocalization)
            {
                if (getRequiredLocalization == null)
                    throw new ArgumentNullException(nameof(getRequiredLocalization));
                var audioExtensions = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var entry in _audioExtensions ?? Array.Empty<AudioExtensionEntry>())
                {
                    if (string.IsNullOrWhiteSpace(entry.AssetId))
                        continue;
                    if (!audioExtensions.TryAdd(entry.AssetId, entry.Extension))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate audio extension override '{entry.AssetId}' "
                            + $"in episode '{_id}'.");
                    }
                }

                return new EpisodeDefinition(
                    contentId,
                    _id,
                    getRequiredLocalization(_titleKey),
                    _storyPath,
                    _contentVersion,
                    new EpisodeMediaDefinition(
                        _videoIds,
                        audioExtensions,
                        _defaultAudioExtension,
                        _silentAudioIds),
                    new EpisodeContentDependencies(
                        _audioDependencies,
                        _backgroundDependencies,
                        _speakerDependencies));
            }
        }

        [SerializeField] private string _id;
        [SerializeField] private string _mainCharacter;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private EpisodeEntry[] _episodes;

        public AudioMixer AudioMixer => _audioMixer;

        public NovelDefinition ToDefinition(Func<string, string> getRequiredLocalization)
        {
            if (getRequiredLocalization == null)
                throw new ArgumentNullException(nameof(getRequiredLocalization));
            return new NovelDefinition(
                _id,
                _mainCharacter,
                (_episodes ?? Array.Empty<EpisodeEntry>())
                    .Select(episode => episode.ToDefinition(
                        _id,
                        getRequiredLocalization)));
        }
    }
}
