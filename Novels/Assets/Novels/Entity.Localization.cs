using Cysharp.Threading.Tasks;
using Disposable;
using Localization;
using UnityEngine;

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
            using (new LoadingPriority.Entity(ThreadPriority.High, _defaultThreadPriority))
                await localization.Init();

            return localization;
        }
    }
}

