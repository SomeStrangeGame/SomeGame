using System;
using System.Collections.Generic;
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
            return false;
            if (IsLoadingInProcess) return false;

            _save.Add(unit);
            using( var cache = new Cache.Entity())
            {
                cache.ByteArrayToCash(_save.ToArray(), _ctx.SaveChoiceFileName);
            }
            return true;
        }

        public bool TryLoadChoice(out byte result)
        {
            result = 255;
            if (!IsLoadingInProcess) return false;
            result = _initSave.First();
            _initSave.RemoveAt(0);
            return true;
        }
    }
}

