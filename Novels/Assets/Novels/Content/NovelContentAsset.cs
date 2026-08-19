using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

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
            [SerializeField] private string _storyPath;
            [SerializeField] private string _contentVersion;
            [SerializeField] private string[] _silentAudioIds;

            internal readonly EpisodeDefinition ToDefinition(string contentId)
            {
                return new EpisodeDefinition(
                    contentId,
                    _id,
                    _title,
                    _storyPath,
                    _contentVersion,
                    new EpisodeMediaDefinition(_silentAudioIds));
            }
        }

        [SerializeField] private string _id;
        [SerializeField] private string _mainCharacter;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private EpisodeEntry[] _episodes;

        public AudioMixer AudioMixer => _audioMixer;

        public NovelDefinition ToDefinition()
        {
            return new NovelDefinition(
                _id,
                _mainCharacter,
                (_episodes ?? Array.Empty<EpisodeEntry>())
                    .Select(episode => episode.ToDefinition(_id)));
        }
    }
}
