using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels.Save
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
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

        private readonly Ctx _ctx;
        private List<byte> _save = new();
        private byte[] _initialChoices = Array.Empty<byte>();
        private int _initialChoicePosition;
        private readonly SaveWriter _writer;

        public bool ContainAnySave => _initialChoices.Length > 0;

        public Entity(Ctx ctx)
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
            _initialChoices = Array.Empty<byte>();
            _initialChoicePosition = 0;
            try
            {
                var data = _ctx.ReadBytes(_ctx.SaveChoiceFileName);
                var decoded = SaveDataCodec.Decode(data);
                if (!MatchesCurrentContent(decoded))
                {
                    _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                        Diagnostics.NovelErrorCodes.SaveContentMismatch,
                        Diagnostics.NovelErrorSeverity.Warning,
                        $"Save content '{decoded.ContentId}@{decoded.ContentVersion}' does not match current content '{_ctx.ContentId}@{_ctx.ContentVersion}'. Replay was skipped."));
                    return;
                }

                _save = decoded.Choices.ToList();
            }
            catch (FileNotFoundException)
            {
                _ctx.OnLog?.Invoke((LogType.Log, "No save file"));
            }
            catch (Exception exception)
            {
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.SaveReadFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    "Failed to read save file. Replay was skipped.",
                    exception: exception));
            }
            _initialChoices = _save.ToArray();
        }

        public byte? GetNextSavedChoice()
        {
            if (_initialChoicePosition >= _initialChoices.Length)
                return null;

            return _initialChoices[_initialChoicePosition++];
        }

        public void SaveChoice(byte unit = 255)
        {
            _save.Add(unit);
            _writer.Enqueue(SaveDataCodec.Encode(
                _ctx.ContentId,
                _ctx.ContentVersion,
                _save.ToArray()));
        }

        public void Clear()
        {
            _writer.Reset(() => _ctx.Delete(_ctx.SaveChoiceFileName));
            _save.Clear();
            _initialChoices = Array.Empty<byte>();
            _initialChoicePosition = 0;
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
