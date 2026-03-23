using System;
using Cysharp.Threading.Tasks;
using Disposable;

namespace Localization
{
    public class Entity: BaseDisposable
    {
        public struct Ctx
        {
            public LocalizationData.Language Language;
            public Func<LocalizationData> GetLocalizationSO;
        }

        private Ctx _ctx;

        private LocalizationData _localizationData;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            _localizationData = _ctx.GetLocalizationSO();
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_localizationData.TryGetValue(_ctx.Language, key, out value))
                return true;

            value = key;
            return false;
        }
    }
}

