using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal static class NovelContentValidator
    {
        private const string _menuPath = "Novels/Validate Content";

        [MenuItem(_menuPath)]
        private static void ValidateFromMenu()
        {
            var errors = ValidateLoadedConfiguration();
            if (errors.Count == 0)
            {
                Debug.Log("Novel content validation completed without errors.");
                return;
            }

            foreach (var error in errors)
                Debug.LogError($"[NovelContent] {error}");
        }

        internal static void ValidateOrThrow()
        {
            var errors = ValidateLoadedConfiguration();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Novel content validation failed:\n- "
                    + string.Join("\n- ", errors));
            }
        }

        private static IReadOnlyList<string> ValidateLoadedConfiguration()
        {
            var errors = new List<string>();
            var entryPoint = UnityEngine.Object.FindFirstObjectByType<Novels.EntryPoint>(
                FindObjectsInactive.Include);
            if (entryPoint == null)
            {
                errors.Add("The loaded scene does not contain Novels.EntryPoint.");
                return errors;
            }

            var serializedEntryPoint = new SerializedObject(entryPoint);
            var data = serializedEntryPoint.FindProperty("_data");
            if (data == null)
            {
                errors.Add("EntryPoint._data cannot be read.");
                return errors;
            }

            var prefix = RequireString(data, "_prefix", errors);
            RequireString(data, "_mainCharacter", errors);
            var storyPath = RequireString(data, "_storyTextPath", errors);

            var bundles = new[]
            {
                RequireString(data, "_novelsLoadingBundleName", errors),
                RequireString(data, "_novelsSettingBundleName", errors),
                RequireString(data, "_novelsBubbleBundleName", errors),
                RequireString(data, "_novelsLocationBundleName", errors),
                RequireString(data, "_novelsCharacterBundleName", errors),
                RequireString(data, "_novelsNotificationBundleName", errors),
                RequireString(data, "_novelsLocalizationBundleName", errors),
            };

            var audioMixer = data.FindPropertyRelative("_audioMixer");
            if (audioMixer?.objectReferenceValue == null)
                errors.Add("AudioMixer is not configured.");

            ValidateStory(prefix, storyPath, errors);
            ValidateBundles(bundles, errors);
            return errors;
        }

        private static string RequireString(
            SerializedProperty parent,
            string propertyName,
            ICollection<string> errors)
        {
            var property = parent.FindPropertyRelative(propertyName);
            var value = property?.stringValue;
            if (string.IsNullOrWhiteSpace(value))
                errors.Add($"EntryPoint data field '{propertyName}' is empty.");
            return value ?? string.Empty;
        }

        private static void ValidateStory(
            string prefix,
            string storyPath,
            ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(prefix)
                || string.IsNullOrWhiteSpace(storyPath))
            {
                return;
            }

            var path = Path.Combine(
                Application.streamingAssetsPath,
                "NovelTexts",
                prefix,
                storyPath);
            if (!File.Exists(path))
                errors.Add($"Compiled Ink story does not exist: {path}");
        }

        private static void ValidateBundles(
            IEnumerable<string> configuredBundles,
            ICollection<string> errors)
        {
            var existingBundles = new HashSet<string>(
                AssetDatabase.GetAllAssetBundleNames(),
                StringComparer.OrdinalIgnoreCase);

            foreach (var bundle in configuredBundles.Where(
                         value => !string.IsNullOrWhiteSpace(value)))
            {
                if (!existingBundles.Contains(bundle))
                    errors.Add($"AssetBundle '{bundle}' is not assigned to any asset.");

                var versionPath = Path.Combine(
                    Application.streamingAssetsPath,
                    "Remote",
                    "Android",
                    bundle,
                    "version.txt");
                if (!File.Exists(versionPath))
                    errors.Add($"Built Android bundle version is missing: {versionPath}");
            }
        }
    }
}
