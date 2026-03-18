using Disposable;
using UnityEngine;

namespace Novels.PathGetter
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public string Prefix;
        }

        private Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public string GetNovelTextPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            return $"NovelTexts/{_ctx.Prefix}/{path}";
        }

        public string GetSettingPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Setting/{_ctx.Prefix}/{assetName}.prefab";
        }

        public string GetBubblePrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Bubble/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        public string GetLocationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        public string GetLocationImagePath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            var firstChar = char.ToUpper(assetName[0]);
            var otherText = assetName.Substring(1).ToLower();
            return $"Assets/Novels/Location/RemoteAssets/{_ctx.Prefix}/Locations/{firstChar}{otherText}.png";
        }

        public string GetVideoPath(string assetName)
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

        public string GetCharacterPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        public string GetCharacterMainBodyPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/{view}/{arg ?? "Main"}.png";
        }

        public string GetCharacterEmotionPath(string name, string view, string arg)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/{view}/Emotions/{firstChar}{otherText}.png";
        }

        public string GetCharacterClothesPath(string name, string arg, int index)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/Clothes/{firstChar}{otherText}/{index}.png";
        }

        public string GetCharacterHairPath(string name, string arg, string direction, string color)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            if (string.IsNullOrEmpty(arg)) return string.Empty;

            var firstChar = char.ToUpper(arg[0]);
            var otherText = arg.Substring(1).ToLower();
            return $"Assets/Novels/Character/RemoteAssets/{_ctx.Prefix}/Characters/{name}/Hairs/{direction}/{firstChar}{otherText}/{color}.png";
        }

        public string GetNotificationPrefabAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/Novels/Notification/RemoteAssets/{_ctx.Prefix}/{assetName}.prefab";
        }

        public string GetLocalizationDataAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return string.Empty;

            return $"Assets/RemoteAssets/Localization/{_ctx.Prefix}/{assetName}.asset";
        }
    }
}

