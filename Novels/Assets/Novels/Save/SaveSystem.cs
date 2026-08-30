using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Save
{
    public class SaveSystem : BaseDisposable
    {
        public struct Dependencies
        {
            public string SaveChoiceFileName;
            public string ContentId;
            public string ContentVersion;
            public Func<string, byte[]> ReadBytes;
            public Action<string, byte[]> WriteBytes;
            public Action<string> Delete;
            public Action<(LogType type, string message)> OnLog;
            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Dependencies _ctx;
        private List<StoryContracts.StoryDecision> _save = new();
        private StoryContracts.StoryDecision[] _initialDecisions =
            Array.Empty<StoryContracts.StoryDecision>();
        private int _initialDecisionPosition;
        private readonly List<SaveDataCodec.WardrobeItem> _wardrobeItems = new();
        private readonly SaveWriter _writer;

        public bool ContainAnySave => _initialDecisions.Length > 0;
        public bool HasUnlockedWardrobeItems => _wardrobeItems.Count > 0;

        public SaveSystem(Dependencies ctx)
        {
            _ctx = ctx;
            _writer = new SaveWriter(
                ctx.SaveChoiceFileName,
                ctx.WriteBytes,
                ReportWriteFailure).AddTo(this);
        }

        public void Init()
        {
            _save.Clear();
            _initialDecisions = Array.Empty<StoryContracts.StoryDecision>();
            _initialDecisionPosition = 0;
            _wardrobeItems.Clear();
            try
            {
                var data = _ctx.ReadBytes(_ctx.SaveChoiceFileName);
                var decoded = SaveDataCodec.Decode(data);
                if (!MatchesCurrentContent(decoded))
                {
                    DiscardIncompatibleSave(
                        $"Save content '{decoded.ContentId}@{decoded.ContentVersion}' "
                        + $"does not match current content "
                        + $"'{_ctx.ContentId}@{_ctx.ContentVersion}'.");
                    return;
                }

                _save = decoded.Decisions.ToList();
                _wardrobeItems.AddRange(decoded.WardrobeItems);
            }
            catch (FileNotFoundException)
            {
                _ctx.OnLog?.Invoke((LogType.Log, "No save file"));
            }
            catch (UnsupportedSaveFormatException exception)
            {
                DiscardIncompatibleSave(
                    $"Save format version '{exception.Version}' is not supported.",
                    exception);
            }
            catch (Exception exception)
            {
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.SaveReadFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    "Failed to read save file. Replay was skipped.",
                    exception: exception));
            }
            _initialDecisions = _save.ToArray();
        }

        private void DiscardIncompatibleSave(string reason, Exception cause = null)
        {
            try
            {
                _ctx.Delete(_ctx.SaveChoiceFileName);
                _ctx.OnLog?.Invoke((
                    LogType.Warning,
                    $"{reason} The incompatible save was deleted. "
                    + "A new game will be started."));
            }
            catch (Exception deleteException)
            {
                var exception = cause == null
                    ? deleteException
                    : new AggregateException(cause, deleteException);
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.SaveReadFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    "Failed to delete an incompatible save file.",
                    exception: exception));
            }
        }

        public StoryContracts.StoryDecision? GetNextSavedDecision()
        {
            if (_initialDecisionPosition >= _initialDecisions.Length)
                return null;

            return _initialDecisions[_initialDecisionPosition++];
        }

        public StoryContracts.StoryDecision[] GetInitialDecisionsSnapshot() =>
            (StoryContracts.StoryDecision[])_initialDecisions.Clone();

        public void DiscardIncompatibleReplay(string reason)
        {
            Clear();
            _ctx.OnLog?.Invoke((
                LogType.Warning,
                $"{reason} The incompatible save was deleted. "
                + "A new game will be started."));
        }

        public void SaveDecision(StoryContracts.StoryDecision decision)
        {
            _save.Add(decision);
            _writer.Enqueue(SaveDataCodec.Encode(
                _ctx.ContentId,
                _ctx.ContentVersion,
                _save.ToArray(),
                _wardrobeItems.ToArray()));
        }

        public void UnlockWardrobeItem(
            string character,
            byte category,
            string value,
            bool persist,
            bool equip = true)
        {
            character = character?.Trim() ?? string.Empty;
            value = value?.Trim() ?? string.Empty;
            if (character.Length == 0 || value.Length == 0)
                return;
            var existing = _wardrobeItems.FindIndex(item =>
                    item.Category == category
                    && string.Equals(item.Character, character, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase));
            if (equip)
            {
                for (var index = 0; index < _wardrobeItems.Count; index++)
                {
                    var item = _wardrobeItems[index];
                    if (item.Category == category
                        && string.Equals(item.Character, character, StringComparison.OrdinalIgnoreCase)
                        && item.Equipped)
                    {
                        _wardrobeItems[index] = new SaveDataCodec.WardrobeItem(
                            item.Character, item.Category, item.Value, false);
                    }
                }
            }
            var updated = new SaveDataCodec.WardrobeItem(
                character, category, value, equip);
            if (existing >= 0)
                _wardrobeItems[existing] = updated;
            else
                _wardrobeItems.Add(updated);
            if (persist)
                EnqueueCurrentSave();
        }

        public string[] GetUnlockedWardrobeItems(
            string character,
            byte category) =>
            _wardrobeItems
                .Where(item => item.Category == category
                    && string.Equals(item.Character, character, StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

        public string GetEquippedWardrobeItem(
            string character,
            byte category) =>
            _wardrobeItems.FirstOrDefault(item => item.Equipped
                && item.Category == category
                && string.Equals(item.Character, character, StringComparison.OrdinalIgnoreCase))
                .Value ?? string.Empty;

        public void RemoveUnavailableWardrobeItems(
            string character,
            byte category,
            IEnumerable<string> availableValues,
            bool persist)
        {
            character = character?.Trim() ?? string.Empty;
            if (character.Length == 0)
                return;
            var available = new HashSet<string>(
                availableValues ?? Array.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            var removed = _wardrobeItems.RemoveAll(item =>
                item.Category == category
                && string.Equals(
                    item.Character,
                    character,
                    StringComparison.OrdinalIgnoreCase)
                && !available.Contains(item.Value));
            if (removed > 0 && persist)
                EnqueueCurrentSave();
        }

        private void EnqueueCurrentSave()
        {
            _writer.Enqueue(SaveDataCodec.Encode(
                _ctx.ContentId,
                _ctx.ContentVersion,
                _save.ToArray(),
                _wardrobeItems.ToArray()));
        }

        public void Clear()
        {
            _writer.Reset(() => _ctx.Delete(_ctx.SaveChoiceFileName));
            _save.Clear();
            _initialDecisions = Array.Empty<StoryContracts.StoryDecision>();
            _initialDecisionPosition = 0;
            _wardrobeItems.Clear();
        }

        public UniTask FlushAsync()
        {
            return _writer.FlushAsync();
        }

        public void FlushSynchronously()
        {
            _writer.FlushSynchronously();
        }

        private void ReportWriteFailure(Exception exception)
        {
            _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                Diagnostics.NovelErrorCodes.SaveWriteFailed,
                Diagnostics.NovelErrorSeverity.Recoverable,
                "Failed to write save file. The current session will continue.",
                exception: exception));
        }

        private bool MatchesCurrentContent(SaveDataCodec.DecodedSave save)
        {
            return string.Equals(
                    save.ContentId,
                    _ctx.ContentId,
                    StringComparison.Ordinal)
                && string.Equals(
                    save.ContentVersion,
                    _ctx.ContentVersion,
                    StringComparison.Ordinal);
        }
    }
}
