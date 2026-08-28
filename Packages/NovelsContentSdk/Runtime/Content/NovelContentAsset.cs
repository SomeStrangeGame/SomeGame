using System;
using System.Linq;
using UnityEngine;

namespace Novels.Content
{
    [CreateAssetMenu(fileName = "NovelContent", menuName = "Novels/Content")]
    public sealed class NovelContentAsset : ScriptableObject
    {
        [Serializable]
        private struct EpisodeEntry
        {
            [SerializeField] private string _id;
            [SerializeField] private string _title;
            [SerializeField] private string _description;

            internal readonly EpisodeDefinition ToDefinition(string contentId)
            {
                return new EpisodeDefinition(
                    contentId,
                    _id,
                    _title,
                    _description);
            }
        }

        [Serializable]
        private struct VideoAliasEntry
        {
            [SerializeField] private string _alias;
            [SerializeField] private string _target;

            internal readonly VideoAliasDefinition ToDefinition() =>
                new(_alias, _target);
        }

        [Serializable]
        private struct ArtAliasEntry
        {
            [SerializeField] private string _alias;
            [SerializeField] private string _target;

            internal readonly ArtAliasDefinition ToDefinition() =>
                new(_alias, _target);
        }

        [Serializable]
        private struct CharacterDefaultEntry
        {
            [SerializeField] private string _character;
            [SerializeField] private string _clothes;
            [SerializeField] private string _hair;
            [SerializeField] private string _hairColor;
            [SerializeField] private string _accessory;

            internal readonly CharacterDefaultAppearanceDefinition ToDefinition() =>
                new(_character, _clothes, _hair, _hairColor, _accessory);
        }

        [Serializable]
        private struct StreamingChunkEntry
        {
            [SerializeField] private string[] _assetGuids;
        }

        [SerializeField] private string _id;
        [SerializeField] private string _mainCharacter;
        [SerializeField] private string _contentVersion = "1";
        [SerializeField] private string _endMarker;
        [SerializeField] private string[] _silentAudioIds;
        [SerializeField] private EpisodeEntry[] _episodes;
        [SerializeField] private VideoAliasEntry[] _videoAliases;
        [SerializeField] private ArtAliasEntry[] _artAliases;
        [SerializeField] private CharacterDefaultEntry[] _characterDefaults;
        [SerializeField, HideInInspector] private string _authoringRootInkGuid;
        [SerializeField, HideInInspector] private int _authoringChunkSizeMiB;
        [SerializeField, HideInInspector] private StreamingChunkEntry[] _authoringChunks;
        [SerializeField, HideInInspector] private string[] _authoringUnusedAssetGuids;

        public NovelDefinition ToDefinition()
        {
            return new NovelDefinition(
                _id,
                _mainCharacter,
                _contentVersion,
                _endMarker,
                _silentAudioIds,
                (_episodes ?? Array.Empty<EpisodeEntry>())
                    .Select(episode => episode.ToDefinition(_id)),
                (_videoAliases ?? Array.Empty<VideoAliasEntry>())
                    .Select(alias => alias.ToDefinition()),
                (_characterDefaults ?? Array.Empty<CharacterDefaultEntry>())
                    .Select(value => value.ToDefinition()),
                (_artAliases ?? Array.Empty<ArtAliasEntry>())
                    .Select(alias => alias.ToDefinition()));
        }
    }
}
