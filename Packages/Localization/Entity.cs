using Disposable;

namespace Localization
{
    public class Entity: BaseDisposable
    {
        public struct Ctx
        {
            public LocalizationData.Language Language;
            public LocalizationData LocalizationSO;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public string GetValue(string key)
        {
            if (_ctx.LocalizationSO.TryGetValue(_ctx.Language, key, out var value))
                return value;
            return key;
        }
    }
}

