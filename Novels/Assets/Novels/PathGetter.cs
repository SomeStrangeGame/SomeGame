using Disposable;
using UnityEngine;

namespace Novels
{
    internal class PathGetter : BaseDisposable
    {
        internal struct Ctx
        {
            internal string Prefix;
        }

        private Ctx _ctx;

        internal PathGetter(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal string GetNovelTextPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return $"NovelTexts/{_ctx.Prefix}/{path}";
        }

        internal string GetSettingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Setting/{_ctx.Prefix}/{assetName}.prefab";
        }

        internal string GetBubblePrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Bubble/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        internal string GetLocationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        internal string GetLocationImagePath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpper(assetName[0]);
            var otherText = assetName.Substring(1).ToLower();
            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Prefix}/Locations/{firstChar}{otherText}.png";
        }

        internal string GetVideoPath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpper(assetName[0]);
            var otherText = assetName.Substring(1).ToLower();
            var result = $"{Application.streamingAssetsPath}/NovelsVideos/{_ctx.Prefix}/{firstChar}{otherText}.mp4";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            result = $"file://{result}";
#endif
            return result;
        }

        internal string GetCharacterPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        internal string GetCharacterMainBodyPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/{view}/{arg ?? "Main"}.png";
        }

        internal string GetCharacterEmotionPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/{view}/Эмоции/{firstChar}{otherText}.png";
        }

        internal string GetCharacterClothesPath(string name, string arg, int index)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/Clothes/{firstChar}{otherText}/{index}.png";
        }

        internal string GetNotificationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Notification/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        internal string GetLocalizationDataAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Localization/{_ctx.Prefix}/{assetName}.asset";
        }
    }
}

