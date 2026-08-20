using System;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal sealed class FallbackAssets
    {
        internal FallbackAssets(
            Sprite background,
            Sprite character,
            GameObject loading,
            GameObject bubble,
            GameObject location,
            GameObject characterScreen,
            GameObject notification)
        {
            Background = background
                ?? throw new ArgumentNullException(nameof(background));
            Character = character
                ?? throw new ArgumentNullException(nameof(character));
            Loading = loading ?? throw new ArgumentNullException(nameof(loading));
            Bubble = bubble ?? throw new ArgumentNullException(nameof(bubble));
            Location = location ?? throw new ArgumentNullException(nameof(location));
            CharacterScreen = characterScreen
                ?? throw new ArgumentNullException(nameof(characterScreen));
            Notification = notification
                ?? throw new ArgumentNullException(nameof(notification));
        }

        internal Sprite Background { get; }
        internal Sprite Character { get; }
        internal GameObject Loading { get; }
        internal GameObject Bubble { get; }
        internal GameObject Location { get; }
        internal GameObject CharacterScreen { get; }
        internal GameObject Notification { get; }
    }

    internal sealed class ApplicationEnvironment
    {
        internal ApplicationEnvironment(
            CancellationToken cancellationToken,
            string persistentDataPath,
            string clientVersion,
            string contentPlatform,
            Camera targetCamera,
            FallbackAssets fallbackAssets,
            NovelRuntimeTuning runtimeTuning)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
                throw new ArgumentException("Persistent data path must not be empty.", nameof(persistentDataPath));
            if (string.IsNullOrWhiteSpace(clientVersion))
                throw new ArgumentException("Client version must not be empty.", nameof(clientVersion));
            if (string.IsNullOrWhiteSpace(contentPlatform))
                throw new ArgumentException("Content platform must not be empty.", nameof(contentPlatform));
            CancellationToken = cancellationToken;
            PersistentDataPath = persistentDataPath;
            ClientVersion = clientVersion;
            ContentPlatform = contentPlatform;
            TargetCamera = targetCamera
                ?? throw new ArgumentNullException(nameof(targetCamera));
            FallbackAssets = fallbackAssets
                ?? throw new ArgumentNullException(nameof(fallbackAssets));
            RuntimeTuning = runtimeTuning;
        }

        internal CancellationToken CancellationToken { get; }
        internal string PersistentDataPath { get; }
        internal string ClientVersion { get; }
        internal string ContentPlatform { get; }
        internal Camera TargetCamera { get; }
        internal FallbackAssets FallbackAssets { get; }
        internal NovelRuntimeTuning RuntimeTuning { get; }
    }
}
