using Cysharp.Threading.Tasks;
using Disposable;
using Localization;

namespace Novels
{
    internal partial class Entity
    {
        private Localization.Entity CreateLocalization(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var localization = new Localization.Entity(new Localization.Entity.Ctx
            {
                Language = LocalizationData.Language.Rus,
                GetLocalizationSO = () => bundles.GetBundledSO<LocalizationData>(_ctx.Data.NovelsLocalizationBundleName, pathGetter.GetLocalizationDataAssetName("LocalizationData")),
            }).AddTo(this);
            localization.Init();

            return localization;
        }
    }
}

