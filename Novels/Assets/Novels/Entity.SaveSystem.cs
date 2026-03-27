using Cysharp.Threading.Tasks;
using Disposable;

namespace Novels
{
    internal partial class Entity
    {
        private Save.Entity CreateSaveSystem()
        {
            var saveSystem = new Save.Entity(new Save.Entity.Ctx
            {
                SaveChoiceFileName = "SaveChoice",
                SaveLocationFileName = "SaveLocation",
                SaveCameraFileName = "SaveCamera",
                OnLog = _ctx.OnLog,
            }).AddTo(this);
            saveSystem.Init();

            return saveSystem;
        }
    }
}

