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

            using (var cache = new Cache.Entity())
            {
                try
                {
                    _save = cache.ByteArrayFromCash(_ctx.SaveChoiceFileName).ToList();
                }
                catch
                {
                    _ctx.OnLog((LogType.Log, "No save file"));
                }
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
            using( var cache = new Cache.Entity())
            {
                cache.ByteArrayToCash(_save.ToArray(), _ctx.SaveChoiceFileName);
            }
        }

        public void Clear()
        {
            var cachPath = $"{Application.persistentDataPath}/CachedFiles/Remote";
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            cachPath = $"file://{cachPath}";
            #endif
            if (Directory.Exists(cachPath))
            {
                Directory.Delete(cachPath, true);
                Debug.Log("Clear cache files done!");
            }
            else
            {
                Debug.Log($"No cache files in {cachPath}");
            }
            _save.Clear();
            _initialChoices = Array.Empty<byte>();
            _initialChoicePosition = 0;
        }
    }
}
