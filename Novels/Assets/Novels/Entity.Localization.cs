using Cysharp.Threading.Tasks;
using Disposable;
using Localization;

namespace Novels
{
    internal partial class Entity
    {
        private async UniTask<Localization.Entity> CreateLocalization(Bundles.Entity bundles, PathGetter.Entity pathGetter)
        {
            var localization = new Localization.Entity(new Localization.Entity.Ctx
            {
                Language = LocalizationData.Language.Rus,
                GetLocalizationSO = () => bundles.GetBundledSO<LocalizationData>(_ctx.Data.NovelsLocalizationBundleName, pathGetter.GetLocalizationDataAssetName("LocalizationData")),
            }).AddTo(this);
            await localization.Init();

            return localization;
        }
    }
}

