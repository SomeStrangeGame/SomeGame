using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Novels.Content
{
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
            [SerializeField] private string _title;
            [SerializeField] private string _storyPath;
            [SerializeField] private string _contentVersion;
            [SerializeField] private string _bubbleBundleName;
            [SerializeField] private string _locationBundleName;
            [SerializeField] private string _characterBundleName;
            [SerializeField] private string _notificationBundleName;
            [SerializeField] private string[] _videoIds;
            [SerializeField] private string _defaultAudioExtension;
            [SerializeField] private string[] _silentAudioIds;
            [SerializeField] private AudioExtensionEntry[] _audioExtensions;

            internal readonly EpisodeDefinition ToDefinition()
            {
                var audioExtensions = new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var entry in _audioExtensions ?? Array.Empty<AudioExtensionEntry>())
                {
                    if (!string.IsNullOrWhiteSpace(entry.AssetId))
                        audioExtensions[entry.AssetId] = entry.Extension;
                }

                return new EpisodeDefinition(
                    _id,
                    _title,
                    _storyPath,
                    _contentVersion,
                    _bubbleBundleName,
                    _locationBundleName,
                    _characterBundleName,
                    _notificationBundleName,
                    new EpisodeMediaDefinition(
                        _videoIds,
                        audioExtensions,
                        _defaultAudioExtension,
                        _silentAudioIds));
            }
        }

        [SerializeField] private string _id;
        [SerializeField] private string _prefix;
        [SerializeField] private string _mainCharacter;
        [SerializeField] private string _mainLoadingBundleName;
        [SerializeField] private string _loadingBundleName;
        [SerializeField] private string _settingBundleName;
        [SerializeField] private string _localizationBundleName;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private EpisodeEntry[] _episodes;

        public AudioMixer AudioMixer => _audioMixer;

        public NovelDefinition ToDefinition()
        {
            return new NovelDefinition(
                _id,
                _prefix,
                _mainCharacter,
                _mainLoadingBundleName,
                _loadingBundleName,
                _settingBundleName,
                _localizationBundleName,
                (_episodes ?? Array.Empty<EpisodeEntry>())
                    .Select(episode => episode.ToDefinition()));
        }
    }
}
