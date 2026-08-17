using System;

namespace Bundles
{
    public static class ContentPlatform
    {
        public static string GetCurrent()
        {
#if UNITY_STANDALONE_OSX
            return "Mac";
#elif UNITY_STANDALONE_WIN
            return "Win";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_ANDROID
            return "Android";
#else
            throw new PlatformNotSupportedException(
                "Content delivery is not configured for the active build target.");
#endif
        }
    }
}
