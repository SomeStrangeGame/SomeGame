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
            var errors = ValidateLoadedConfiguration(true);
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
            ValidateOrThrow(true);
            Debug.Log("Novel content batch validation completed without errors.");
        }

        internal static void ValidateOrThrow()
        {
            ValidateOrThrow(false);
        }

        internal static void ValidateBuiltOutputOrThrow()
        {
            ValidateOrThrow(true);
        }

        private static void ValidateOrThrow(bool validateBuiltOutput)
        {
            var errors = ValidateLoadedConfiguration(validateBuiltOutput);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Novel content validation failed:\n- "
                    + string.Join("\n- ", errors));
            }
        }

        private static IReadOnlyList<string> ValidateLoadedConfiguration(
            bool validateBuiltOutput)
        {
            var errors = new List<string>();
            var entryPoint = UnityEngine.Object.FindFirstObjectByType<Novels.EntryPoint>(
                FindObjectsInactive.Include);
            if (entryPoint == null)
            {
                errors.Add("The loaded scene does not contain Novels.EntryPoint.");
                return errors;
            }

            var catalog = AssetDatabase.LoadAssetAtPath<Novels.Catalog.NovelCatalogAsset>(
                Novels.Catalog.CatalogAddresses.AssetName);
            if (catalog == null)
            {
                errors.Add(
                    $"Novel catalog does not exist: "
                    + Novels.Catalog.CatalogAddresses.AssetName);
                return errors;
            }

            ValidateBundleAssignment(
                Novels.Catalog.CatalogAddresses.AssetName,
                Novels.Catalog.CatalogAddresses.BundleName,
                errors);
            var catalogScreen = AssetDatabase.LoadAssetAtPath<GameObject>(
                Novels.Catalog.CatalogAddresses.ScreenAssetName);
            if (catalogScreen == null)
            {
                errors.Add(
                    $"Catalog screen prefab does not exist: "
                    + Novels.Catalog.CatalogAddresses.ScreenAssetName);
            }
            else
            {
                ValidateCatalogScreen(catalogScreen, errors);
                ValidateBundleAssignment(
                    Novels.Catalog.CatalogAddresses.ScreenAssetName,
                    Novels.Catalog.CatalogAddresses.BundleName,
                    errors);
            }

            if (string.IsNullOrWhiteSpace(catalog.Resolve().Title))
                errors.Add("Novel catalog title is empty.");
            if (catalog.Entries.Count == 0)
                errors.Add("Novel catalog has no entries.");

            var contentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.ContentId))
                {
                    errors.Add("Novel catalog contains an entry without content ID.");
                    continue;
                }
                if (!contentIds.Add(entry.ContentId))
                    errors.Add($"Duplicate catalog content ID: {entry.ContentId}");
                if (string.IsNullOrWhiteSpace(entry.Resolve().Title))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no title.");
                if (string.IsNullOrWhiteSpace(entry.ContentBundleName))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no content bundle.");
                if (string.IsNullOrWhiteSpace(entry.ContentAssetName))
                    errors.Add($"Catalog entry '{entry.ContentId}' has no content asset address.");

                var contentAsset = AssetDatabase.LoadAssetAtPath<
                    Novels.Content.NovelContentAsset>(entry.ContentAssetName);
                if (contentAsset == null)
                {
                    errors.Add(
                        $"NovelContentAsset does not exist: {entry.ContentAssetName}");
                    continue;
                }

                ValidateBundleAssignment(
                    entry.ContentAssetName,
                    entry.ContentBundleName,
                    errors);
                ValidateContentAsset(
                    contentAsset,
                    entry.ContentId,
                    entry.ContentBundleName,
                    validateBuiltOutput,
                    errors);
            }
            ValidateBootstrapPrefab(errors);
            if (validateBuiltOutput)
                ValidateReleaseManifest(errors);
            return errors;
        }

        private static void ValidateCatalogScreen(
            GameObject prefab,
            ICollection<string> errors)
        {
            var screen = prefab.GetComponent<Novels.Catalog.View.Screen>();
            if (screen == null)
            {
                errors.Add("Catalog screen prefab has no Catalog.View.Screen component.");
                return;
            }

            var serializedScreen = new SerializedObject(screen);
            foreach (var propertyName in new[] { "_title", "_cardPrefab" })
            {
                if (serializedScreen.FindProperty(propertyName)?.objectReferenceValue == null)
                    errors.Add($"Catalog screen prefab has no '{propertyName}' reference.");
            }
            var card = serializedScreen.FindProperty("_cardPrefab")?.objectReferenceValue
                as Novels.Catalog.View.Card;
            if (card != null)
            {
                var serializedCard = new SerializedObject(card);
                foreach (var propertyName in new[]
                         {
                             "_title",
                             "_description",
                             "_status",
                             "_button",
                         })
                {
                    if (serializedCard.FindProperty(propertyName)?.objectReferenceValue == null)
                        errors.Add($"Catalog card prefab has no '{propertyName}' reference.");
                }
            }
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Catalog screen prefab root has zero scale.");
            var viewport = prefab.transform.Find("Content/Viewport");
            if (viewport == null || viewport.GetComponent<UnityEngine.UI.RectMask2D>() == null)
                errors.Add("Catalog screen viewport must use RectMask2D.");
        }

        private static void ValidateBootstrapPrefab(ICollection<string> errors)
        {
            const string path = "Assets/Resources/Novels/BootstrapScreen.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add($"Local bootstrap prefab is missing: {path}");
                return;
            }

            var screen = prefab.GetComponent<Novels.Bootstrap.View.Screen>();
            if (screen == null)
            {
                errors.Add($"Local bootstrap prefab has no Screen component: {path}");
                return;
            }

            var serializedScreen = new SerializedObject(screen);
            foreach (var propertyName in new[] { "_message", "_retryLabel", "_retry" })
            {
                if (serializedScreen.FindProperty(propertyName)?.objectReferenceValue == null)
                    errors.Add($"Local bootstrap prefab has no '{propertyName}' reference.");
            }
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Local bootstrap prefab root has zero scale.");
        }

        private static void ValidateReleaseManifest(ICollection<string> errors)
        {
            var remoteRoot = Path.Combine(
                Application.streamingAssetsPath,
                "Remote",
                "Android");
            var path = Path.Combine(remoteRoot, "release.json");
            if (!File.Exists(path))
            {
                errors.Add($"Built Android content release is missing: {path}");
                return;
            }

            Bundles.ContentRelease release;
            try
            {
                release = JsonUtility.FromJson<Bundles.ContentRelease>(
                    File.ReadAllText(path));
            }
            catch (Exception exception)
            {
                errors.Add($"Content release cannot be parsed: {exception.Message}");
                return;
            }
            if (release == null || string.IsNullOrWhiteSpace(release.releaseId))
            {
                errors.Add("Content release ID is empty.");
                return;
            }
            try
            {
                Bundles.ContentReleaseValidator.Validate(
                    release,
                    Application.version,
                    1);
            }
            catch (Exception exception)
            {
                errors.Add($"Content release is invalid: {exception.Message}");
                return;
            }

            foreach (var releaseBundle in release.bundles)
            {
                if (releaseBundle == null
                    || string.IsNullOrWhiteSpace(releaseBundle.name))
                {
                    errors.Add("Release contains a bundle without a name.");
                    continue;
                }
                var versionPath = Path.Combine(
                    remoteRoot,
                    releaseBundle.name,
                    "version.txt");
                if (!File.Exists(versionPath))
                {
                    errors.Add(
                        $"Release bundle is missing: '{releaseBundle.name}'.");
                }
                else if (!string.Equals(
                        File.ReadAllText(versionPath).Trim(),
                        releaseBundle.version,
                        StringComparison.Ordinal))
                {
                    errors.Add(
                        $"Release version does not match bundle "
                        + $"'{releaseBundle.name}'.");
                }
            }

            foreach (var file in ContentFilePolicy.EnumerateFiles())
            {
                var relative = ContentFilePolicy.GetRelativePath(file);
                if (release.FindFile(relative) == null)
                    errors.Add($"Release does not describe file '{relative}'.");
            }

        }

        private static void ValidateBundleAssignment(
            string assetPath,
            string expectedBundle,
            ICollection<string> errors)
        {
            var assignedBundle = AssetDatabase.GetImplicitAssetBundleName(assetPath);
            if (!string.Equals(
                    assignedBundle,
                    expectedBundle,
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Asset '{assetPath}' must belong to "
                    + $"AssetBundle '{expectedBundle}'.");
            }
        }

        private static void ValidateContentAsset(
            Novels.Content.NovelContentAsset contentAsset,
            string expectedContentId,
            string contentBundleName,
            bool validateBuiltOutput,
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
            if (!string.Equals(
                    definition.Id,
                    expectedContentId,
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    $"Catalog content ID '{expectedContentId}' does not match "
                    + $"NovelContentAsset ID '{definition.Id}'.");
            }

            ValidateBundles(
                new[]
                {
                    Novels.Catalog.CatalogAddresses.BundleName,
                    contentBundleName,
                    definition.MainLoadingBundleName,
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
                validateBuiltOutput,
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
            bool validateBuiltOutput,
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

                if (validateBuiltOutput)
                {
                    var versionPath = Path.Combine(
                        Application.streamingAssetsPath,
                        "Remote",
                        "Android",
                        bundle,
                        "version.txt");
                    if (!File.Exists(versionPath))
                        errors.Add($"Built Android bundle version is missing: {versionPath}");
                    var manifestPath = Path.Combine(
                        Application.streamingAssetsPath,
                        "Remote",
                        "Android",
                        bundle,
                        "manifest.json");
                    if (!File.Exists(manifestPath))
                        errors.Add($"Built Android bundle manifest is missing: {manifestPath}");
                }
            }
        }
    }
}
