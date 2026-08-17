using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private const string _saveChoiceFileName = "SaveChoice";

        private Save.Entity CreateSaveSystem()
        {
            var cache = new Cache.Entity(Application.persistentDataPath)
                .AddTo(this);
            var saveSystem = new Save.Entity(new Save.Entity.Ctx
            {
                SaveChoiceFileName = _saveChoiceFileName,
                ContentId = $"{_definition.Id}/{_episode.Id}",
                ContentVersion = _episode.ContentVersion,
                ReadBytes = cache.ReadBytes,
                WriteBytes = cache.WriteBytes,
                Delete = cache.Delete,
                OnLog = _ctx.OnLog,
                OnError = _ctx.OnError,
            }).AddTo(this);
            saveSystem.Init();
            _saveSystem = saveSystem;

            return saveSystem;
        }
    }
}
