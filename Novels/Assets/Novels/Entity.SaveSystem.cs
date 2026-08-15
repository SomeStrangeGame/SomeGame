using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private const string _saveChoiceFileName = "SaveChoice";

        private Save.Entity CreateSaveSystem()
        {
            var cache = new Cache.Entity().AddTo(this);
            var saveSystem = new Save.Entity(new Save.Entity.Ctx
            {
                SaveChoiceFileName = _saveChoiceFileName,
                ContentId = $"{_definition.Id}/{_definition.Episode.Id}",
                ContentVersion = _definition.Episode.ContentVersion,
                ReadBytes = cache.ReadBytes,
                WriteBytes = cache.WriteBytes,
                Delete = cache.Delete,
                OnLog = _ctx.OnLog,
                OnError = _ctx.OnError,
            }).AddTo(this);
            saveSystem.Init();

            return saveSystem;
        }
    }
}
