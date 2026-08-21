using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    internal static class PrefabContentValidator
    {
        internal static void ValidateCatalog(GameObject prefab, ContentValidationReport errors)
        {
            var screen = prefab.GetComponent<Novels.Catalog.View.CatalogScreen>();
            if (screen == null)
            {
                errors.Add("Catalog screen prefab has no Catalog.View.CatalogScreen component.");
                return;
            }
            var serializedScreen = new SerializedObject(screen);
            ValidateReferences(serializedScreen, "Catalog screen prefab", errors, "_title", "_cardPrefab");
            var card = serializedScreen.FindProperty("_cardPrefab")?.objectReferenceValue
                as Novels.Catalog.View.Card;
            if (card != null)
            {
                ValidateReferences(
                    new SerializedObject(card),
                    "Catalog card prefab",
                    errors,
                    "_title",
                    "_description",
                    "_status",
                    "_button");
            }
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Catalog screen prefab root has zero scale.");
            var viewport = prefab.transform.Find("Content/Viewport");
            if (viewport == null || viewport.GetComponent<RectMask2D>() == null)
                errors.Add("Catalog screen viewport must use RectMask2D.");
        }

        internal static void ValidateBootstrap(ContentValidationReport errors)
        {
            const string path = "Assets/Resources/Novels/BootstrapScreen.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add($"Local bootstrap prefab is missing: {path}");
                return;
            }
            var screen = prefab.GetComponent<Novels.Bootstrap.View.BootstrapScreen>();
            if (screen == null)
            {
                errors.Add($"Local bootstrap prefab has no Screen component: {path}");
                return;
            }
            ValidateReferences(
                new SerializedObject(screen),
                "Local bootstrap prefab",
                errors,
                "_message",
                "_retryLabel",
                "_retry");
            if (prefab.transform.localScale == Vector3.zero)
                errors.Add("Local bootstrap prefab root has zero scale.");
        }

        internal static void ValidateEpisode(
            string prefix,
            string episodeId,
            ContentValidationReport errors)
        {
            var assetName = Novels.ContentAddressing.ContentAssetNames.EpisodeScreen;
            ValidateLoading(
                ResolvePresentationPath(
                    Novels.ContentAddressing.ContentAddressConvention.LoadingPrefab(
                        prefix, episodeId, assetName),
                    Novels.ContentAddressing.ContentAddressConvention.SharedLoadingPrefab(
                        prefix, assetName),
                    "Loading",
                    prefix,
                    episodeId,
                    errors),
                errors);
            ValidateBubble(
                ResolvePresentationPath(
                    Novels.ContentAddressing.ContentAddressConvention.BubblePrefab(
                        prefix, episodeId, assetName),
                    Novels.ContentAddressing.ContentAddressConvention.SharedBubblePrefab(
                        prefix, assetName),
                    "Bubble",
                    prefix,
                    episodeId,
                    errors),
                errors);
            ValidateCharacter(
                ResolvePresentationPath(
                    Novels.ContentAddressing.ContentAddressConvention.CharacterPrefab(
                        prefix, episodeId, assetName),
                    Novels.ContentAddressing.ContentAddressConvention.SharedCharacterPrefab(
                        prefix, assetName),
                    "Character",
                    prefix,
                    episodeId,
                    errors),
                errors);
            ValidateLocation(
                ResolvePresentationPath(
                    Novels.ContentAddressing.ContentAddressConvention.LocationPrefab(
                        prefix, episodeId, assetName),
                    Novels.ContentAddressing.ContentAddressConvention.SharedLocationPrefab(
                        prefix, assetName),
                    "Location",
                    prefix,
                    episodeId,
                    errors),
                errors);
            ValidateNotification(
                ResolvePresentationPath(
                    Novels.ContentAddressing.ContentAddressConvention.NotificationPrefab(
                        prefix, episodeId, assetName),
                    Novels.ContentAddressing.ContentAddressConvention.SharedNotificationPrefab(
                        prefix, assetName),
                    "Notification",
                    prefix,
                    episodeId,
                    errors),
                errors);
        }

        internal static void ValidateFallbackEpisode(ContentValidationReport errors)
        {
            const string root = "Assets/Novels/Fallbacks/EpisodeUI";
            var assetName = Novels.ContentAddressing.ContentAssetNames.EpisodeScreen;
            ValidateLoading($"{root}/loading/{assetName}.prefab", errors);
            ValidateBubble($"{root}/bubble/{assetName}.prefab", errors);
            ValidateCharacter($"{root}/character/{assetName}.prefab", errors);
            ValidateLocation($"{root}/location/{assetName}.prefab", errors);
            ValidateNotification($"{root}/notification/{assetName}.prefab", errors);
        }

        private static string ResolvePresentationPath(
            string episodePath,
            string sharedPath,
            string kind,
            string contentId,
            string episodeId,
            ContentValidationReport report)
        {
            if (AssetDatabase.LoadMainAssetAtPath(episodePath) != null)
                return episodePath;
            if (AssetDatabase.LoadMainAssetAtPath(sharedPath) != null)
                return sharedPath;
            report.Add(ContentValidationIssue.Warning(
                ContentValidationCodes.EpisodeUiPrefabMissing,
                $"{kind} screen prefab is absent from both episode and shared content. "
                + "The built-in fallback prefab will be used.",
                contentId: contentId,
                episodeId: episodeId));
            return null;
        }

        private static void ValidateLoading(string path, ContentValidationReport errors)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var screen = LoadScreen<Loading.View.Screen>(path, "Loading", errors);
            if (screen != null)
            {
                ValidateReferences(
                    new SerializedObject(screen),
                    $"Loading screen prefab '{path}'",
                    errors,
                    "_marker",
                    "_canvasGroup");
            }
        }

        private static void ValidateBubble(string path, ContentValidationReport errors)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var screen = LoadScreen<Novels.Bubble.View.BubbleScreen>(path, "Bubble", errors);
            if (screen == null)
                return;
            var serialized = new SerializedObject(screen);
            ValidateReferences(
                serialized,
                $"Bubble screen prefab '{path}'",
                errors,
                "_bubblesView._root",
                "_bubblesView._buttonPrefab",
                "_bubblesView._backgroundButton",
                "_canvasGroup");
            var bubbles = FindProperty(serialized, "_bubblesView._bubbles");
            if (bubbles == null || !bubbles.isArray || bubbles.arraySize == 0)
            {
                errors.Add($"Bubble screen prefab '{path}' has no bubble presentations.");
                return;
            }
            for (var index = 0; index < bubbles.arraySize; index++)
            {
                var bubble = bubbles.GetArrayElementAtIndex(index);
                ValidateReference(
                    bubble.FindPropertyRelative("_root"),
                    $"Bubble screen prefab '{path}' bubble {index}",
                    "_root",
                    errors);
                ValidateReference(
                    bubble.FindPropertyRelative("_header"),
                    $"Bubble screen prefab '{path}' bubble {index}",
                    "_header",
                    errors);
                ValidateReference(
                    bubble.FindPropertyRelative("_text"),
                    $"Bubble screen prefab '{path}' bubble {index}",
                    "_text",
                    errors);
            }
        }

        private static void ValidateCharacter(string path, ContentValidationReport errors)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var screen = LoadScreen<Novels.Character.View.CharacterScreen>(
                path,
                "Character",
                errors);
            if (screen != null)
            {
                ValidateReferences(
                    new SerializedObject(screen),
                    $"Character screen prefab '{path}'",
                    errors,
                    "_canvasGroup",
                    "_mainBody",
                    "_clothes",
                    "_emotion",
                    "_backHairs",
                    "_frontHairs",
                    "_backAccessories",
                    "_middleAccessories",
                    "_frontAccessories");
            }
        }

        private static void ValidateLocation(string path, ContentValidationReport errors)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var screen = LoadScreen<Novels.Location.View.LocationScreen>(path, "Location", errors);
            if (screen == null)
                return;
            var serialized = new SerializedObject(screen);
            ValidateReferences(
                serialized,
                $"Location screen prefab '{path}'",
                errors,
                "_imageCanvasGroup",
                "_image",
                "_video",
                "_videoImage",
                "_effectCanvasGroup");
            var effects = serialized.FindProperty("_effects");
            if (effects == null || !effects.isArray || effects.arraySize == 0)
            {
                errors.Add($"Location screen prefab '{path}' has no visual effects.");
                return;
            }
            for (var index = 0; index < effects.arraySize; index++)
            {
                ValidateReference(
                    effects.GetArrayElementAtIndex(index)
                        .FindPropertyRelative("_effectRoot"),
                    $"Location screen prefab '{path}' effect {index}",
                    "_effectRoot",
                    errors);
            }
        }

        private static void ValidateNotification(
            string path,
            ContentValidationReport errors)
        {
            if (string.IsNullOrEmpty(path))
                return;
            var screen = LoadScreen<Novels.Notification.View.NotificationScreen>(
                path,
                "Notification",
                errors);
            if (screen != null)
            {
                ValidateReferences(
                    new SerializedObject(screen),
                    $"Notification screen prefab '{path}'",
                    errors,
                    "_text",
                    "_canvasGroup");
            }
        }

        private static T LoadScreen<T>(
            string path,
            string kind,
            ContentValidationReport errors)
            where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                errors.Add($"{kind} screen prefab does not exist: {path}");
                return null;
            }
            var screen = prefab.GetComponent<T>();
            if (screen == null)
                errors.Add($"{kind} screen prefab '{path}' has no {typeof(T).FullName} component.");
            return screen;
        }

        private static void ValidateReferences(
            SerializedObject target,
            string owner,
            ContentValidationReport errors,
            params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                ValidateReference(
                    FindProperty(target, propertyName),
                    owner,
                    propertyName,
                    errors);
            }
        }

        private static SerializedProperty FindProperty(
            SerializedObject target,
            string path)
        {
            var segments = path.Split('.');
            var property = target.FindProperty(segments[0]);
            for (var index = 1; property != null && index < segments.Length; index++)
                property = property.FindPropertyRelative(segments[index]);
            return property;
        }

        private static void ValidateReference(
            SerializedProperty property,
            string owner,
            string propertyName,
            ContentValidationReport errors)
        {
            if (property == null
                || property.propertyType != SerializedPropertyType.ObjectReference
                || property.objectReferenceValue == null)
            {
                errors.Add($"{owner} has no '{propertyName}' reference.");
            }
        }
    }
}
