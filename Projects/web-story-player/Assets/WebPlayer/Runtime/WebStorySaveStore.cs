using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.WebPlayer
{
    // Save acknowledgement means the IndexedDB transaction has committed.
    // Content cache is disposable and must not share the save database.
    public sealed class WebStorySaveStore : MonoBehaviour
    {
        private readonly Dictionary<string, UniTaskCompletionSource<string>> _pending = new();
        private long _sequence;

        [Serializable]
        private sealed class Result
        {
            public string id;
            public string value;
            public bool found;
            public string error;
        }

        public UniTask<string> Read(string storyId, string version) =>
            Request("read", Key(storyId, version), "");

        public async UniTask Write(string storyId, string version, string payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            await Request("write", Key(storyId, version), payload);
        }

        public async UniTask Delete(string storyId, string version) =>
            await Request("delete", Key(storyId, version), "");

        private static string Key(string storyId, string version)
        {
            if (string.IsNullOrWhiteSpace(storyId) || string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Story and version are required for saves.");
            return Uri.EscapeDataString(storyId) + "/" + Uri.EscapeDataString(version);
        }

        private UniTask<string> Request(string operation, string key, string value)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var id = (++_sequence).ToString();
            var completion = new UniTaskCompletionSource<string>();
            _pending.Add(id, completion);
            try { SomeGameWebSave(gameObject.name, id, operation, key, value); }
            catch (Exception error)
            {
                _pending.Remove(id);
                completion.TrySetException(error);
            }
            return completion.Task;
#else
            var editorKey = "web-story-save/" + key;
            if (operation == "write") { PlayerPrefs.SetString(editorKey, value); PlayerPrefs.Save(); }
            if (operation == "delete") { PlayerPrefs.DeleteKey(editorKey); PlayerPrefs.Save(); }
            return UniTask.FromResult(operation == "read" && PlayerPrefs.HasKey(editorKey)
                ? PlayerPrefs.GetString(editorKey) : null);
#endif
        }

        public void OnSaveResult(string json)
        {
            var result = JsonUtility.FromJson<Result>(json);
            if (result == null || !_pending.Remove(result.id, out var completion)) return;
            if (!string.IsNullOrEmpty(result.error))
                completion.TrySetException(new InvalidOperationException(result.error));
            else completion.TrySetResult(result.found ? result.value : null);
        }

        private void OnDestroy()
        {
            foreach (var completion in _pending.Values) completion.TrySetCanceled();
            _pending.Clear();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SomeGameWebSave(
            string target, string id, string operation, string key, string value);
#endif
    }
}
