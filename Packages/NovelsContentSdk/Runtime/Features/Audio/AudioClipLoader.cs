using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels.Audio
{
    internal sealed class AudioClipLoader
    {
        private readonly CancellationToken _cancellationToken;

        internal AudioClipLoader(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<AudioClip> Load(string url, bool stream)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip(
                url,
                GetAudioType(url));
            var handler = (DownloadHandlerAudioClip)request.downloadHandler;
            handler.streamAudio = stream;
            await request.SendWebRequest().WithCancellation(_cancellationToken);
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException(
                    $"Audio request failed [{request.responseCode}] {url}: {request.error}");
            }
            return DownloadHandlerAudioClip.GetContent(request);
        }

        private static AudioType GetAudioType(string url)
        {
            if (url.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                return AudioType.MPEG;
            if (url.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                return AudioType.OGGVORBIS;
            return AudioType.WAV;
        }
    }
}
