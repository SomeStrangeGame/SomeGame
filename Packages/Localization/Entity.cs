using System;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Localization
{
    public class Entity: BaseDisposable
    {
        public struct Ctx
        {
            public Func<UniTask<LocalizationData>> GetLocalizationSO;
        }

        private Ctx _ctx;

        private LocalizationData _localizationData;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            _localizationData = await _ctx.GetLocalizationSO();
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_localizationData.TryGetValue(key, out value))
                return true;
            return false;
        }
    }
}

