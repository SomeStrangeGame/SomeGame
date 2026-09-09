using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TrinadtsatyyKolokol.Editor
{
    // Editor-only authoring action; never edits serialized YAML or build output.
    internal static class BindApprovedEpisodeCovers
    {
        [MenuItem("Novels/Story/Trinadtsatyy Kolokol/Bind Approved Episode Covers")]
        private static void Bind()
        {
            const string assetPath = "Assets/trinadtsatyy-kolokol.asset";
            var definition = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (definition == null)
                throw new InvalidOperationException("Story definition is missing.");
            var serialized = new SerializedObject(definition);
            if (serialized.FindProperty("_id")?.stringValue != "trinadtsatyy-kolokol")
                throw new InvalidOperationException("Unexpected story identity.");
            var episodes = serialized.FindProperty("_episodes");
            if (episodes == null || !episodes.isArray || episodes.arraySize != 6)
                throw new InvalidOperationException("Expected exactly six approved episodes.");
            var covers = new Dictionary<string, string>();
            for (var number = 1; number <= 6; number++)
            {
                var id = $"s01e{number:00}";
                var filename = id + ".png";
                var path = Path.Combine(Application.dataPath, "..", "Config", "EpisodeCovers", filename);
                if (!File.Exists(path))
                    throw new InvalidOperationException($"Approved cover is missing: {filename}");
                covers.Add(id, filename);
            }
            var seen = new HashSet<string>();
            for (var index = 0; index < episodes.arraySize; index++)
            {
                var episode = episodes.GetArrayElementAtIndex(index);
                var id = episode.FindPropertyRelative("_id")?.stringValue;
                if (id == null || !covers.ContainsKey(id) || !seen.Add(id)
                    || episode.FindPropertyRelative("_catalogCover") == null)
                    throw new InvalidOperationException("Unexpected episode ID or cover schema.");
            }
            Undo.RecordObject(definition, "Bind approved episode covers");
            for (var index = 0; index < episodes.arraySize; index++)
            {
                var episode = episodes.GetArrayElementAtIndex(index);
                episode.FindPropertyRelative("_catalogCover").stringValue =
                    covers[episode.FindPropertyRelative("_id").stringValue];
            }
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssetIfDirty(definition);
            Debug.Log("Trinadtsatyy Kolokol: six approved episode covers bound by stable ID.");
        }
    }
}
