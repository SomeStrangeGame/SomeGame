using Disposable;
using UnityEngine;
using System;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private const string _saveChoiceFileName = "SaveChoice";

        private Save.SaveSystem CreateSaveSystem()
        {
            var cache = new Cache.Entity(_ctx.PersistentDataPath);
            var saveKey = $"Saves/{Uri.EscapeDataString(_definition.Id)}/"
                + $"{Uri.EscapeDataString(_episode.Id)}/{_saveChoiceFileName}";
            var saveSystem = new Save.SaveSystem(new Save.SaveSystem.Dependencies
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
