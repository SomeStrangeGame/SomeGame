using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Novels
{
    /// <summary>
    /// Owns Ink entry states at authored episode boundaries. These are progression
    /// snapshots, not user-created checkpoints.
    /// </summary>
    public sealed class NovelProgress
    {
        private static readonly byte[] _magic = { 0x4E, 0x50, 0x52, 0x31 };
        private const byte _formatVersion = 1;

        private readonly string _contentId;
        private readonly string _contentVersion;
        private readonly IReadOnlyList<Content.EpisodeDefinition> _episodes;
        private readonly bool _resetIncompatible;
        private readonly bool _strict;
        private readonly string _key;
        private readonly Func<string, byte[]> _read;
        private readonly Action<string, byte[]> _write;
        private readonly Action<string> _delete;
        private readonly Func<string, bool> _exists;
        private readonly Action<(LogType type, string message)> _log;
        private readonly Dictionary<string, string> _entryStates =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _completedEpisodes =
            new(StringComparer.OrdinalIgnoreCase);

        public NovelProgress(
            Content.NovelDefinition definition,
            string persistentDataPath,
            Action<(LogType type, string message)> log)
            : this((definition ?? throw new ArgumentNullException(nameof(definition))).Id,
                definition.ContentVersion, definition.Episodes, persistentDataPath, log)
        {
        }

        public NovelProgress(
            string contentId,
            string contentVersion,
            IReadOnlyList<Content.EpisodeDefinition> episodes,
            string persistentDataPath,
            Action<(LogType type, string message)> log,
            bool resetIncompatible = true)
            : this(contentId, contentVersion, episodes, new Cache.Entity(persistentDataPath),
                log, resetIncompatible)
        {
        }

        private NovelProgress(string contentId, string contentVersion,
            IReadOnlyList<Content.EpisodeDefinition> episodes, Cache.Entity cache,
            Action<(LogType type, string message)> log, bool resetIncompatible)
            : this(contentId, contentVersion, episodes, cache.ReadBytes, cache.WriteBytes,
                cache.Delete, cache.Exists, log, resetIncompatible)
        {
        }

        public NovelProgress(string contentId, string contentVersion,
            IReadOnlyList<Content.EpisodeDefinition> episodes,
            Func<string, byte[]> read, Action<string, byte[]> write,
            Action<string> delete, Func<string, bool> exists,
            Action<(LogType type, string message)> log = null,
            bool resetIncompatible = false, bool strict = false)
        {
            _contentId = contentId;
            _contentVersion = contentVersion;
            _episodes = episodes;
            _resetIncompatible = resetIncompatible;
            _strict = strict;
            _key = $"Saves/{Uri.EscapeDataString(_contentId)}/Progress";
            _read = read;
            _write = write;
            _delete = delete;
            _exists = exists;
            _log = log;
            Load();
        }

        public IReadOnlyList<Content.EpisodeDefinition> PlayableEpisodes
        {
            get
            {
                var count = 1;
                while (count < _episodes.Count
                    && _entryStates.ContainsKey(_episodes[count].Id))
                {
                    count++;
                }
                return _episodes.Take(count).ToArray();
            }
        }

        public string GetEntryState(Content.EpisodeDefinition episode) =>
            _entryStates.TryGetValue(episode.Id, out var state) ? state : null;

        public IReadOnlyCollection<string> CompletedEpisodeIds => _completedEpisodes;

        public void Begin(Content.EpisodeDefinition episode)
        {
            var index = IndexOf(episode.Id);
            var changed = false;
            for (var position = index; position < _episodes.Count; position++)
            {
                var affected = _episodes[position].Id;
                if (_completedEpisodes.Remove(affected))
                    _delete(CompletionKey(affected));
            }
            for (var position = index + 1; position < _episodes.Count; position++)
                changed |= _entryStates.Remove(_episodes[position].Id);
            if (changed)
                Save();
        }

        public void Complete(Content.EpisodeDefinition episode, string continuationState)
        {
            if (string.IsNullOrWhiteSpace(continuationState))
            {
                MarkCompleted(episode.Id);
                return;
            }
            MarkCompleted(episode.Id);
            var next = IndexOf(episode.Id) + 1;
            if (next >= _episodes.Count)
                return;
            _entryStates[_episodes[next].Id] = continuationState;
            Save();
        }

        private int IndexOf(string episodeId)
        {
            for (var index = 0; index < _episodes.Count; index++)
            {
                if (string.Equals(
                    _episodes[index].Id,
                    episodeId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }
            throw new InvalidOperationException($"Unknown episode '{episodeId}'.");
        }

        private void Load()
        {
            foreach (var episode in _episodes)
            {
                if (_exists(CompletionKey(episode.Id)))
                    _completedEpisodes.Add(episode.Id);
            }
            try
            {
                Decode(_read(_key));
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception exception)
            {
                if (_strict) throw new InvalidDataException("Novel progress is incompatible; preserved.", exception);
                _entryStates.Clear();
                try
                {
                    if (_resetIncompatible) _delete(_key);
                }
                catch (Exception deleteException)
                {
                    exception = new AggregateException(exception, deleteException);
                }
                _log?.Invoke((
                    LogType.Warning,
                    $"Novel progress is incompatible ({(_resetIncompatible ? "reset" : "read-only preview; preserved")}): {exception.Message}"));
            }
        }

        private void Save() => _write(_key, Encode());

        private void MarkCompleted(string episodeId)
        {
            if (!_completedEpisodes.Add(episodeId))
                return;
            _write(CompletionKey(episodeId), new byte[] { 1 });
        }

        private string CompletionKey(string episodeId) =>
            $"Saves/{Uri.EscapeDataString(_contentId)}/"
            + $"Completed/{Uri.EscapeDataString(episodeId)}";

        private string ContentVersion => _contentVersion;

        private string LegacyContentVersion => string.Join(
            "|",
            _episodes.Select(episode =>
                $"{episode.Id}:{_contentVersion}"));

        private byte[] Encode()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(_magic);
            writer.Write(_formatVersion);
            writer.Write(_contentId);
            writer.Write(ContentVersion);
            writer.Write(_entryStates.Count);
            foreach (var entry in _entryStates.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
            return stream.ToArray();
        }

        private void Decode(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes ?? Array.Empty<byte>(), false);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            if (!reader.ReadBytes(_magic.Length).SequenceEqual(_magic)
                || reader.ReadByte() != _formatVersion
                || !string.Equals(reader.ReadString(), _contentId, StringComparison.Ordinal)
                || !MatchesContentVersion(reader.ReadString()))
            {
                throw new InvalidDataException("Novel progress envelope is incompatible.");
            }

            var count = reader.ReadInt32();
            if (count < 0 || count > _episodes.Count - 1)
                throw new InvalidDataException("Novel progress entry count is invalid.");
            for (var index = 0; index < count; index++)
            {
                var episodeId = reader.ReadString();
                var state = reader.ReadString();
                if (IndexOf(episodeId) == 0 || string.IsNullOrWhiteSpace(state))
                    throw new InvalidDataException("Novel progress entry is invalid.");
                _entryStates.Add(episodeId, state);
            }
            if (stream.Position != stream.Length)
                throw new InvalidDataException("Novel progress has trailing data.");
        }

        private bool MatchesContentVersion(string value) =>
            string.Equals(value, ContentVersion, StringComparison.Ordinal)
            || string.Equals(value, LegacyContentVersion, StringComparison.Ordinal);
    }
}
