using System;

namespace Bundles
{
    public static class ContentPlatform
    {
        public static string GetCurrent()
        {
#if UNITY_EDITOR_OSX
            // The active build profile can define UNITY_ANDROID or UNITY_IOS while
            // the macOS Editor still needs bundles built for its host platform.
            return "Mac";
#elif UNITY_STANDALONE_OSX
            return "Mac";
#elif UNITY_STANDALONE_WIN
            return "Win";
#elif UNITY_WEBGL
            return "WebGL";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            throw new PlatformNotSupportedException(
                "Content delivery is not configured for the active build target.");
#endif
        }
    }
}
