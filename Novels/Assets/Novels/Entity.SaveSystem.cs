using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Save.Entity> CreateSaveSystem()
        {
            var saveSystem = new Save.Entity(new Save.Entity.Ctx
            {
                SaveFileName = "Save",
                OnLog = _ctx.OnLog,
            }).AddTo(this);
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await saveSystem.Init();

            return saveSystem;
        }
    }
}

