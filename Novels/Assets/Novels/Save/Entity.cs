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
        private List<byte> _initSave = new();

        public bool IsLoadingInProcess => _initSave.Count > 0;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
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
            _initSave = _save.ToList();
        }

        public bool TrySaveChoice(byte unit = 255)
        {
            if (IsLoadingInProcess) return false;

            _save.Add(unit);
            using( var cache = new Cache.Entity())
            {
                cache.ByteArrayToCash(_save.ToArray(), _ctx.SaveChoiceFileName);
            }
            return true;
        }

        public byte LoadChoice()
        {
            var result = _initSave.First();
            _initSave.RemoveAt(0);
            return result;
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
            _initSave.Clear();
        }
    }
}

