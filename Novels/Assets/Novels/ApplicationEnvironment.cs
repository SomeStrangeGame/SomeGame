using System;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal sealed class ApplicationEnvironment
    {
        internal ApplicationEnvironment(
            CancellationToken cancellationToken,
            string persistentDataPath,
            string clientVersion,
            string locale,
            string contentPlatform,
            Camera targetCamera,
            NovelRuntimeTuning runtimeTuning)
        {
            if (string.IsNullOrWhiteSpace(persistentDataPath))
                throw new ArgumentException("Persistent data path must not be empty.", nameof(persistentDataPath));
            if (string.IsNullOrWhiteSpace(clientVersion))
                throw new ArgumentException("Client version must not be empty.", nameof(clientVersion));
            if (string.IsNullOrWhiteSpace(locale))
                throw new ArgumentException("Locale must not be empty.", nameof(locale));
            if (string.IsNullOrWhiteSpace(contentPlatform))
                throw new ArgumentException("Content platform must not be empty.", nameof(contentPlatform));
            CancellationToken = cancellationToken;
            PersistentDataPath = persistentDataPath;
            ClientVersion = clientVersion;
            Locale = locale;
            ContentPlatform = contentPlatform;
            TargetCamera = targetCamera
                ?? throw new ArgumentNullException(nameof(targetCamera));
            RuntimeTuning = runtimeTuning;
        }

        internal CancellationToken CancellationToken { get; }
        internal string PersistentDataPath { get; }
        internal string ClientVersion { get; }
        internal string Locale { get; }
        internal string ContentPlatform { get; }
        internal Camera TargetCamera { get; }
        internal NovelRuntimeTuning RuntimeTuning { get; }
    }
}
