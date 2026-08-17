using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        public static void ValidateBatch()
        {
            EditorSceneManager.OpenScene("Assets/Novels/Novels.unity", OpenSceneMode.Single);
            ValidateOrThrow();
            Debug.Log("Novel content batch validation completed without errors.");
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
            var contentProperty = serializedEntryPoint.FindProperty("_content");
            if (contentProperty == null)
            {
                errors.Add("EntryPoint._content cannot be read.");
                return errors;
            }

            if (contentProperty.objectReferenceValue is Novels.Content.NovelContentAsset contentAsset)
                ValidateContentAsset(contentAsset, errors);
            else
                errors.Add("EntryPoint has no NovelContentAsset configured.");
            return errors;
        }

        private static void ValidateContentAsset(
            Novels.Content.NovelContentAsset contentAsset,
            ICollection<string> errors)
        {
            Novels.Content.NovelDefinition definition;
            try
            {
                definition = contentAsset.ToDefinition();
            }
            catch (Exception exception)
            {
                errors.Add($"Content asset '{contentAsset.name}' is invalid: {exception.Message}");
                return;
            }

            if (contentAsset.AudioMixer == null)
                errors.Add($"Content asset '{contentAsset.name}' has no AudioMixer.");

            ValidateBundles(
                new[]
                {
                    definition.LoadingBundleName,
                    definition.SettingBundleName,
                    definition.LocalizationBundleName,
                }.Concat(definition.Episodes.SelectMany(episode => new[]
                {
                    episode.BubbleBundleName,
                    episode.LocationBundleName,
                    episode.CharacterBundleName,
                    episode.NotificationBundleName,
                })),
                errors);

            foreach (var episode in definition.Episodes)
            {
                ValidateStory(definition.Prefix, episode.StoryPath, errors);
                ValidateStorySyntax(definition.Prefix, episode, errors);
                ValidateMedia(definition.Prefix, episode, errors);
            }
        }

        private static void ValidateMedia(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            foreach (var videoId in episode.Media.VideoIds)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "NovelsVideos",
                    prefix,
                    videoId + ".mp4");
                if (!File.Exists(path))
                    errors.Add($"Configured video does not exist: {path}");
            }

            foreach (var audio in episode.Media.AudioExtensions)
            {
                var path = Path.Combine(
                    Application.streamingAssetsPath,
                    "NovelsAudio",
                    prefix,
                    audio.Key + audio.Value);
                if (!File.Exists(path))
                    errors.Add($"Configured audio override does not exist: {path}");
            }
        }

        private static void ValidateStorySyntax(
            string prefix,
            Novels.Content.EpisodeDefinition episode,
            ICollection<string> errors)
        {
            var path = Path.Combine(
                Application.streamingAssetsPath,
                "NovelTexts",
                prefix,
                episode.StoryPath);
            if (!File.Exists(path))
                return;

            var parser = new Novels.StoryCommands.Entity();
            var json = File.ReadAllText(path);
            var matches = Regex.Matches(
                json,
                "\"\\^(?<text>(?:\\\\.|[^\"\\\\])*)\"");
            foreach (Match match in matches)
            {
                var source = Regex.Unescape(match.Groups["text"].Value);
                if (!source.Contains(":"))
                    continue;
                var result = parser.Parse(source, false);
                if (!result.IsSuccess)
                {
                    errors.Add(
                        $"Story command [{result.Error.Code}] in '{episode.StoryPath}': "
                        + $"{result.Error.Message} Source: {source}");
                }
                else if (result.Command is Novels.StoryCommands.AudioStoryCommand audio)
                {
                    if (episode.Media.SilentAudioIds.Contains(
                            audio.Data.AssetName,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    var extension = Path.GetExtension(audio.Data.AssetName);
                    if (extension.Length == 0)
                    {
                        extension = episode.Media.AudioExtensions.TryGetValue(
                            audio.Data.AssetName,
                            out var configuredExtension)
                            ? configuredExtension
                            : episode.Media.DefaultAudioExtension;
                    }
                    var audioPath = Path.Combine(
                        Application.streamingAssetsPath,
                        "NovelsAudio",
                        prefix,
                        audio.Data.AssetName + (Path.GetExtension(audio.Data.AssetName).Length == 0
                            ? extension
                            : string.Empty));
                    if (!File.Exists(audioPath))
                        errors.Add($"Story audio does not exist: {audioPath}");
                }
            }
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
