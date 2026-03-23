using System;
using System.Collections.Generic;
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
            public string SaveFileName;
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
                    _save = cache.ByteArrayFromCash(_ctx.SaveFileName).ToList();
                }
                catch
                {
                    _ctx.OnLog((LogType.Log, "No save file"));
                }
            }
            _initSave = _save.ToList();
        }

        public bool TrySave(byte unit = 255)
        {
            if (IsLoadingInProcess) return false;

            _save.Add(unit);
            using( var cache = new Cache.Entity())
            {
                cache.ByteArrayToCash(_save.ToArray(), "Save");
            }
            return true;
        }

        public bool TryLoad(out byte result)
        {
            result = 255;
            if (!IsLoadingInProcess) return false;
            result = _initSave.First();
            _initSave.RemoveAt(0);
            return true;
        }
    }
}

