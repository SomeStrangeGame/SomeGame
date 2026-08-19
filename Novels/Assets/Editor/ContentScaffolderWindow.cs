using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    internal sealed class ContentScaffolderWindow : EditorWindow
    {
        private const string _menuPath = "Novels/Content/Create Story";

        private string _contentId = "NEW_STORY";
        private string _storyTitle = "New story";
        private string _mainCharacter = "MainCharacter";
        private string _episodeId = "s01e01";
        private string _episodeTitle = "Episode 1";

        [MenuItem(_menuPath)]
        private static void Open()
        {
            var window = GetWindow<ContentScaffolderWindow>(true, "Create Novel Story");
            window.minSize = new Vector2(420f, 210f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Story package", EditorStyles.boldLabel);
            _contentId = EditorGUILayout.TextField("Content ID", _contentId);
            _storyTitle = EditorGUILayout.TextField("English title", _storyTitle);
            _mainCharacter = EditorGUILayout.TextField("Main character", _mainCharacter);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Initial episode", EditorStyles.boldLabel);
            _episodeId = EditorGUILayout.TextField("Episode ID", _episodeId);
            _episodeTitle = EditorGUILayout.TextField("Episode title", _episodeTitle);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(
                       string.IsNullOrWhiteSpace(_contentId)
                       || string.IsNullOrWhiteSpace(_storyTitle)
                       || string.IsNullOrWhiteSpace(_mainCharacter)
                       || string.IsNullOrWhiteSpace(_episodeId)
                       || string.IsNullOrWhiteSpace(_episodeTitle)))
            {
                if (GUILayout.Button("Create story package"))
                    Create();
            }
        }

        private void Create()
        {
            string createdContentRoot = null;
            string createdStoryRoot = null;
            string catalogBackup = null;
            Novels.Catalog.NovelCatalogAsset catalog = null;
            try
            {
                var contentId = _contentId.Trim();
                var episodeId = _episodeId.Trim();
                var definitionPath =
                    Novels.ContentAddressing.ContentPackageConvention.DefinitionAsset(
                        contentId);
                if (AssetDatabase.LoadMainAssetAtPath(definitionPath) != null)
                {
                    throw new InvalidOperationException(
                        $"Content definition already exists: {definitionPath}");
                }

                catalog = AssetDatabase.LoadAssetAtPath<
                    Novels.Catalog.NovelCatalogAsset>(
                    Novels.Catalog.CatalogAddresses.AssetName)
                    ?? throw new InvalidOperationException("Novel catalog asset is missing.");
                if (catalog.Entries.Any(entry => entry != null && string.Equals(
                        entry.ContentId,
                        contentId,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException(
                        $"Catalog already contains content '{contentId}'.");
                }

                var contentRoot =
                    Novels.ContentAddressing.ContentPackageConvention.ContentRoot(contentId);
                if (AssetDatabase.IsValidFolder(contentRoot))
                {
                    throw new InvalidOperationException(
                        $"Content root already exists: {contentRoot}");
                }
                var episodeRoot =
                    Novels.ContentAddressing.ContentPackageConvention.EpisodeRoot(
                        contentId,
                        episodeId);
                var storyRoot = $"Assets/StreamingAssets/NovelTexts/{contentId}";
                if (AssetDatabase.IsValidFolder(storyRoot)
                    || Directory.Exists(Path.GetFullPath(storyRoot)))
                {
                    throw new InvalidOperationException(
                        $"Story text root already exists: {storyRoot}");
                }

                catalogBackup = EditorJsonUtility.ToJson(catalog);
                EnsureFolder($"{contentRoot}/Definition");
                createdContentRoot = contentRoot;
                EnsureFolder($"{contentRoot}/Application/Setting");
                foreach (var feature in new[]
                         {
                             "Loading",
                             "Bubble",
                             "Character",
                             "Location",
                             "Notification",
                         })
                {
                    EnsureFolder($"{episodeRoot}/{feature}");
                }
                EnsureFolder(storyRoot);
                createdStoryRoot = storyRoot;
                File.WriteAllText(
                    Path.GetFullPath($"{storyRoot}/{episodeId}.ink"),
                    $"// {_storyTitle.Trim()}\n{_episodeTitle.Trim()}\n-> END\n");

                SetBundleLabel(
                    contentRoot,
                    Novels.ContentAddressing.ContentPackageConvention.ContentBundle(
                        contentId));
                SetBundleLabel(
                    episodeRoot,
                    Novels.ContentAddressing.ContentPackageConvention.EpisodeBundle(
                        contentId,
                        episodeId));

                var content = CreateInstance<Novels.Content.NovelContentAsset>();
                AssetDatabase.CreateAsset(content, definitionPath);
                Undo.RegisterCreatedObjectUndo(content, "Create novel content");
                ConfigureContent(content, contentId, episodeId);
                AppendCatalogEntry(catalog, contentId);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ContentProjectIndex.BuildOrThrow();
                Selection.activeObject = content;
                EditorGUIUtility.PingObject(content);
                Debug.Log(
                    $"Novel story package '{contentId}' created at '{contentRoot}'.");
            }
            catch (Exception exception)
            {
                Rollback(
                    createdContentRoot,
                    createdStoryRoot,
                    catalog,
                    catalogBackup);
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Create Novel Story",
                    exception.Message,
                    "Close");
            }
        }

        private static void Rollback(
            string contentRoot,
            string storyRoot,
            Novels.Catalog.NovelCatalogAsset catalog,
            string catalogBackup)
        {
            try
            {
                if (catalog != null && !string.IsNullOrEmpty(catalogBackup))
                {
                    EditorJsonUtility.FromJsonOverwrite(catalogBackup, catalog);
                    EditorUtility.SetDirty(catalog);
                }
                if (!string.IsNullOrEmpty(contentRoot))
                    AssetDatabase.DeleteAsset(contentRoot);
                if (!string.IsNullOrEmpty(storyRoot))
                    AssetDatabase.DeleteAsset(storyRoot);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception rollbackException)
            {
                Debug.LogError($"Novel story scaffold rollback failed: {rollbackException}");
            }
        }

        private void ConfigureContent(
            Novels.Content.NovelContentAsset content,
            string contentId,
            string episodeId)
        {
            var serialized = new SerializedObject(content);
            serialized.FindProperty("_id").stringValue = contentId;
            serialized.FindProperty("_mainCharacter").stringValue = _mainCharacter.Trim();
            var episodes = serialized.FindProperty("_episodes");
            episodes.arraySize = 1;
            var episode = episodes.GetArrayElementAtIndex(0);
            episode.FindPropertyRelative("_id").stringValue = episodeId;
            episode.FindPropertyRelative("_title").stringValue = _episodeTitle.Trim();
            episode.FindPropertyRelative("_storyPath").stringValue = episodeId + ".ink.json";
            episode.FindPropertyRelative("_contentVersion").stringValue = "1";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(content);
        }

        private void AppendCatalogEntry(
            Novels.Catalog.NovelCatalogAsset catalog,
            string contentId)
        {
            Undo.RecordObject(catalog, "Add novel catalog entry");
            var serialized = new SerializedObject(catalog);
            var entries = serialized.FindProperty("_entries");
            var index = entries.arraySize;
            entries.InsertArrayElementAtIndex(index);
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("_contentId").stringValue = contentId;
            entry.FindPropertyRelative("_title").stringValue = _storyTitle.Trim();
            entry.FindPropertyRelative("_description").stringValue = string.Empty;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, segments[index]);
                current = next;
            }
        }

        private static void SetBundleLabel(string folder, string bundleName)
        {
            var importer = AssetImporter.GetAtPath(folder)
                ?? throw new InvalidOperationException(
                    $"Asset importer is missing for folder '{folder}'.");
            importer.assetBundleName = bundleName;
            importer.SaveAndReimport();
        }
    }
}
