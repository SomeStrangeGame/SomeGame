using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using System;

namespace Novels
{
    internal partial class Entity
    {
        private const string _saveChoiceFileName = "SaveChoice";

        private Save.Entity CreateSaveSystem()
        {
            var cache = new Cache.Entity(_ctx.PersistentDataPath)
                .AddTo(this);
            var saveKey = $"Saves/{Uri.EscapeDataString(_definition.Id)}/"
                + $"{Uri.EscapeDataString(_episode.Id)}/{_saveChoiceFileName}";
            var saveSystem = new Save.Entity(new Save.Entity.Ctx
            {
                SaveChoiceFileName = saveKey,
                ContentId = $"{_definition.Id}/{_episode.Id}",
                ContentVersion = _episode.ContentVersion,
                ReadBytes = cache.ReadBytes,
                WriteBytes = cache.WriteBytes,
                Delete = cache.Delete,
                OnLog = _ctx.OnLog,
                OnError = ReportError,
            }).AddTo(this);
            saveSystem.Init();
            _saveSystem = saveSystem;

            return saveSystem;
        }
    }
}
