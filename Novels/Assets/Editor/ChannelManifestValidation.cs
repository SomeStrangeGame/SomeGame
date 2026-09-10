using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ChannelManifestValidation
    {
        [MenuItem("Tools/Novels/Validate Channel Manifest Contract")]
        public static void Run()
        {
            var manifest = Novels.ChannelManifest.Deserialize(
                "{\"schema\":1,\"stories\":{\"somestory\":\"1.0\",\"forest-story\":\"2.1\"}}");
            Require(manifest.StoryIds.Count == 2, "Story count differs.");
            Require(manifest.StoryIds[0] == "somestory", "Story order differs.");
            Require(manifest.StoryRoot("forest-story")
                == "stories/forest-story/2.1", "Story root differs.");
            Require(Novels.ChannelManifest.FileName("prod") == "prod.json",
                "Channel file differs.");
            RequireInvalid("{}", "Missing schema was accepted.");
            RequireInvalid("{\"schema\":1,\"stories\":{}}", "Empty stories were accepted.");
            RequireInvalid(
                "{\"schema\":1,\"stories\":{\"somestory\":\"../prod\"}}",
                "Unsafe version was accepted.");
            Debug.Log("Channel manifest contract validation passed.");
        }

        private static void RequireInvalid(string json, string message)
        {
            try
            {
                Novels.ChannelManifest.Deserialize(json);
            }
            catch (InvalidOperationException)
            {
                return;
            }
            throw new InvalidOperationException(message);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
