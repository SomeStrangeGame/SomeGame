using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.WebPlayer
{
    // One IndexedDB record atomically stores the native progress blob and completion
    // markers. Native NPR1 payloads are unchanged; the browser envelope is versioned.
    internal sealed class WebNovelProgress
    {
        [Serializable] private sealed class Entry { public string key; public string value; }
        [Serializable] private sealed class Envelope
        {
            public int schema;
            public string contentId;
            public string contentVersion;
            public Entry[] entries;
        }

        private readonly Dictionary<string, byte[]> _records = new(StringComparer.Ordinal);
        private readonly WebStorySaveStore _store;
        private readonly Content.NovelDefinition _definition;
        private readonly string _releaseVersion;
        private bool _dirty;

        private WebNovelProgress(WebStorySaveStore store, Content.NovelDefinition definition,
            string releaseVersion)
        {
            _store = store;
            _definition = definition;
            _releaseVersion = releaseVersion;
        }

        // Decision keys always contain a slash; this reserved key cannot collide.
        private string Key => "$progress$" + Uri.EscapeDataString(_definition.Id);
        internal NovelProgress Progress { get; private set; }

        internal static async UniTask<WebNovelProgress> Open(WebStorySaveStore store,
            Content.NovelDefinition definition, string releaseVersion)
        {
            var session = new WebNovelProgress(store, definition, releaseVersion);
            var json = await store.Read(session.Key, releaseVersion);
            if (json != null) session.Decode(json);
            session.Progress = new NovelProgress(definition.Id, definition.ContentVersion,
                definition.Episodes,
                key => session._records.TryGetValue(key, out var bytes)
                    ? (byte[])bytes.Clone() : throw new FileNotFoundException(),
                (key, bytes) => { session._records[key] = (byte[])bytes.Clone(); session._dirty = true; },
                key => { session._dirty |= session._records.Remove(key); },
                session._records.ContainsKey, strict: true);
            return session;
        }

        internal async UniTask FlushAsync()
        {
            if (!_dirty) return;
            var envelope = new Envelope
            {
                schema = 1, contentId = _definition.Id, contentVersion = _definition.ContentVersion,
                entries = _records.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new Entry { key = pair.Key, value = Convert.ToBase64String(pair.Value) }).ToArray(),
            };
            // Called only after the episode run has drained. Failure leaves dirty
            // state intact so StopAsync/Launch can retry the identical boundary.
            await _store.Write(Key, _releaseVersion, JsonUtility.ToJson(envelope));
            _dirty = false;
        }

        private void Decode(string json)
        {
            var envelope = JsonUtility.FromJson<Envelope>(json);
            if (envelope == null || envelope.schema != 1
                || envelope.contentId != _definition.Id || envelope.contentVersion != _definition.ContentVersion
                || envelope.entries == null || envelope.entries.Length > _definition.Episodes.Count + 1)
                throw new InvalidDataException("Browser progress envelope is incompatible; preserved.");
            var prefix = "Saves/" + Uri.EscapeDataString(_definition.Id) + "/";
            var completionKeys = new HashSet<string>(_definition.Episodes
                .Select(episode => prefix + "Completed/" + Uri.EscapeDataString(episode.Id)), StringComparer.Ordinal);
            foreach (var entry in envelope.entries)
            {
                if (entry == null || entry.key == null || entry.value == null
                    || (entry.key != prefix + "Progress" && !completionKeys.Contains(entry.key)))
                    throw new InvalidDataException("Browser progress record is invalid; preserved.");
                var bytes = Convert.FromBase64String(entry.value);
                if (completionKeys.Contains(entry.key) && (bytes.Length != 1 || bytes[0] != 1))
                    throw new InvalidDataException("Browser completion marker is invalid; preserved.");
                _records.Add(entry.key, bytes);
            }
        }
    }
}
