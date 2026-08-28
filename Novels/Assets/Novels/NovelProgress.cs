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
    internal sealed class NovelProgress
    {
        private static readonly byte[] _magic = { 0x4E, 0x50, 0x52, 0x31 };
        private const byte _formatVersion = 1;

        private readonly Content.NovelDefinition _definition;
        private readonly string _key;
        private readonly Func<string, byte[]> _read;
        private readonly Action<string, byte[]> _write;
        private readonly Action<string> _delete;
        private readonly Action<(LogType type, string message)> _log;
        private readonly Dictionary<string, string> _entryStates =
            new(StringComparer.OrdinalIgnoreCase);

        internal NovelProgress(
            Content.NovelDefinition definition,
            string persistentDataPath,
            Action<(LogType type, string message)> log)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var cache = new Cache.Entity(persistentDataPath);
            _key = $"Saves/{Uri.EscapeDataString(definition.Id)}/Progress";
            _read = cache.ReadBytes;
            _write = cache.WriteBytes;
            _delete = cache.Delete;
            _log = log;
            Load();
        }

        internal IReadOnlyList<Content.EpisodeDefinition> PlayableEpisodes
        {
            get
            {
                var count = 1;
                while (count < _definition.Episodes.Count
                    && _entryStates.ContainsKey(_definition.Episodes[count].Id))
                {
                    count++;
                }
                return _definition.Episodes.Take(count).ToArray();
            }
        }

        internal string GetEntryState(Content.EpisodeDefinition episode) =>
            _entryStates.TryGetValue(episode.Id, out var state) ? state : null;

        internal void Begin(Content.EpisodeDefinition episode)
        {
            var index = IndexOf(episode.Id);
            var changed = false;
            for (var position = index + 1; position < _definition.Episodes.Count; position++)
                changed |= _entryStates.Remove(_definition.Episodes[position].Id);
            if (changed)
                Save();
        }

        internal void Complete(Content.EpisodeDefinition episode, string continuationState)
        {
            if (string.IsNullOrWhiteSpace(continuationState))
                return;
            var next = IndexOf(episode.Id) + 1;
            if (next >= _definition.Episodes.Count)
                return;
            _entryStates[_definition.Episodes[next].Id] = continuationState;
            Save();
        }

        private int IndexOf(string episodeId)
        {
            for (var index = 0; index < _definition.Episodes.Count; index++)
            {
                if (string.Equals(
                    _definition.Episodes[index].Id,
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
            try
            {
                Decode(_read(_key));
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception exception)
            {
                _entryStates.Clear();
                try
                {
                    _delete(_key);
                }
                catch (Exception deleteException)
                {
                    exception = new AggregateException(exception, deleteException);
                }
                _log?.Invoke((
                    LogType.Warning,
                    $"Novel progress is incompatible and was reset: {exception.Message}"));
            }
        }

        private void Save() => _write(_key, Encode());

        private string ContentVersion => _definition.ContentVersion;

        private string LegacyContentVersion => string.Join(
            "|",
            _definition.Episodes.Select(episode =>
                $"{episode.Id}:{_definition.ContentVersion}"));

        private byte[] Encode()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(_magic);
            writer.Write(_formatVersion);
            writer.Write(_definition.Id);
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
                || !string.Equals(reader.ReadString(), _definition.Id, StringComparison.Ordinal)
                || !MatchesContentVersion(reader.ReadString()))
            {
                throw new InvalidDataException("Novel progress envelope is incompatible.");
            }

            var count = reader.ReadInt32();
            if (count < 0 || count > _definition.Episodes.Count - 1)
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
