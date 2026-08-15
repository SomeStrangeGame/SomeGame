using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Disposable;
using UnityEngine;

namespace Novels.Save
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string SaveChoiceFileName;
            public Func<string, byte[]> ReadBytes;
            public Action<string, byte[]> WriteBytes;
            public Action<string> Delete;
            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;
        private List<byte> _save = new();
        private byte[] _initialChoices = Array.Empty<byte>();
        private int _initialChoicePosition;

        public bool ContainAnySave => _initialChoices.Length > 0;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            _save.Clear();
            _initialChoices = Array.Empty<byte>();
            _initialChoicePosition = 0;

            try
            {
                _save = _ctx.ReadBytes(_ctx.SaveChoiceFileName).ToList();
            }
            catch (FileNotFoundException)
            {
                _ctx.OnLog((LogType.Log, "No save file"));
            }
            catch (Exception exception)
            {
                _ctx.OnLog((LogType.Error, $"Failed to read save file: {exception.Message}"));
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
            _ctx.WriteBytes(_ctx.SaveChoiceFileName, _save.ToArray());
        }

        public void Clear()
        {
            _ctx.Delete(_ctx.SaveChoiceFileName);
            _save.Clear();
            _initialChoices = Array.Empty<byte>();
            _initialChoicePosition = 0;
        }
    }
}
