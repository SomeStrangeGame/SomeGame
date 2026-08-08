using Cysharp.Threading.Tasks;
using Disposable;
using Localization;

namespace Novels
{
    internal partial class Entity
    {
        private Localization.Entity CreateLocalization(LocalizationData localizationSO)
        {
            return new Localization.Entity(new Localization.Entity.Ctx
            {
                Language = LocalizationData.Language.Rus,
                LocalizationSO = localizationSO,
            }).AddTo(this);
        }
    }
}

