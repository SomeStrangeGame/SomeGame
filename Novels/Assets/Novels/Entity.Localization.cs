using Localization;

namespace Novels
{
    internal partial class Entity
    {
        private Localization.Entity CreateLocalization(LocalizationData localizationSO)
        {
            return new Localization.Entity(new Localization.Entity.Ctx
            {
                Locale = _ctx.Locale,
                LocalizationSO = localizationSO,
            });
        }
    }
}
