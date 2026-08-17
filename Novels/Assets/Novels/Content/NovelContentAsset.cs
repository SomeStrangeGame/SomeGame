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
        private struct CharacterAssetProfileEntry
        {
            [SerializeField] private string _mainCharacterAssetId;
            [SerializeField] private string _viewRoot;
            [SerializeField] private string _childView;
            [SerializeField] private string _backLayer;
            [SerializeField] private string _middleLayer;
            [SerializeField] private string _frontLayer;
            [SerializeField] private string _defaultHairColor;

            internal readonly CharacterAssetProfile ToProfile() =>
                new(
                    _mainCharacterAssetId,
                    _viewRoot,
                    _childView,
                    _backLayer,
                    _middleLayer,
                    _frontLayer,
                    _defaultHairColor);
        }

        [Serializable]
        private struct EpisodeEntry
        {
            [SerializeField] private string _id;
            [SerializeField] private string _title;
            [SerializeField] private LocalizedTextEntry[] _localizations;
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
                string locale)
            {
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
                    LocalizedText.Resolve(_localizations, _title, locale),
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

        [Serializable]
        private struct LocalizedTextEntry
        {
            [SerializeField] private string _locale;
            [SerializeField] private string _value;

            internal readonly string Locale => _locale;
            internal readonly string Value => _value;
        }

        private static class LocalizedText
        {
            internal static string Resolve(
                LocalizedTextEntry[] entries,
                string fallback,
                string locale)
            {
                var values = entries ?? Array.Empty<LocalizedTextEntry>();
                var found = Locale.LocaleSelector.TryFind(
                    values,
                    entry => entry.Locale,
                    locale,
                    out var selected);
                return found && !string.IsNullOrWhiteSpace(selected.Value)
                    ? selected.Value
                    : fallback;
            }
        }

        [SerializeField] private string _id;
        [SerializeField] private string _mainCharacter;
        [SerializeField] private CharacterAssetProfileEntry _characterAssets;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private EpisodeEntry[] _episodes;

        public AudioMixer AudioMixer => _audioMixer;

        public NovelDefinition ToDefinition(string locale)
        {
            return new NovelDefinition(
                _id,
                _mainCharacter,
                (_episodes ?? Array.Empty<EpisodeEntry>())
                    .Select(episode => episode.ToDefinition(_id, locale)),
                _characterAssets.ToProfile());
        }
    }
}
