using System;
using UnityEngine;

namespace Novels
{
    internal sealed class StoryStreamingProgressOverlay : MonoBehaviour
    {
        private const string _fallbackResource =
            "Fallbacks/StoryStreamingProgress/screen";

        private StoryStreamingProgressScreen _screen;

        internal static StoryStreamingProgressOverlay Create()
        {
            var prefab = Resources.Load<GameObject>(_fallbackResource);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Story streaming progress prefab '{_fallbackResource}' is missing.");
            }
            var instance = Instantiate(prefab);
            instance.name = nameof(StoryStreamingProgressOverlay);
            return instance.GetComponent<StoryStreamingProgressOverlay>()
                ?? throw new InvalidOperationException(
                    "Story streaming progress prefab has no overlay controller.");
        }

        private void Awake()
        {
            _screen = GetComponent<StoryStreamingProgressScreen>()
                ?? throw new InvalidOperationException(
                    "Story streaming progress prefab has no screen view.");
        }

        internal void Report(float ratio) =>
            _screen.SetProgress(ratio);

        internal void Complete() => _screen.SetComplete();

        internal void Interrupted(float ratio) => _screen.SetInterrupted(ratio);
    }
}
