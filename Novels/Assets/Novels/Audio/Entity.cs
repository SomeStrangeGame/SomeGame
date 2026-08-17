using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace Novels.Audio
{
    public class Entity : BaseDisposable
    {
        private const string _musicMixerGroup = "Music";
        private const string _soundMixerGroup = "Sound";
        private const string _ambientMixerGroup = "Ambient";

        public enum Audio
        {
            Music,
            Sound,
            Ambient
        }

        public struct Ctx
        {
            public Func<string, UniTask<string>> ResolveAudioUrl;
            public AudioMixer AudioMixer;
            public CancellationToken CancellationToken;

            public Action<(LogType type, string message)> OnLog;
            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Ctx _ctx;

        private const int _soundCacheCapacity = 8;
        private readonly Dictionary<Audio, AudioSource> _audioSources;
        private readonly Dictionary<string, AudioClip> _soundClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _soundClipOrder = new();

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _audioSources = new();
        }
        
        public async UniTask PlayAudio(string assetName, Audio type)
        {
            try
            {
                var audioURL = await _ctx.ResolveAudioUrl(assetName);
                if (string.IsNullOrEmpty(audioURL))
                    throw new FileNotFoundException($"Audio '{assetName}' is absent from the media manifest.");

                var audioClip = type == Audio.Sound && _soundClips.TryGetValue(assetName, out var cached)
                    ? cached
                    : null;
                if (audioClip == null)
                {
                    using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(
                               audioURL,
                               GetAudioType(audioURL)))
                    {
                        var handler = (DownloadHandlerAudioClip)audioRequest.downloadHandler;
                        handler.streamAudio = type != Audio.Sound;
                        await audioRequest.SendWebRequest().WithCancellation(_ctx.CancellationToken);
                        if (audioRequest.result != UnityWebRequest.Result.Success)
                        {
                            throw new InvalidOperationException(
                                $"Audio request failed [{audioRequest.responseCode}] {audioURL}: {audioRequest.error}");
                        }

                        audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                    }
                    if (type == Audio.Sound)
                        CacheSound(assetName, audioClip);
                }
                UpdateAudioSource(assetName, audioClip, type);
                _ctx.OnLog?.Invoke((LogType.Log, $"Play audio {assetName}"));
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                ClearAudio(type);
                _ctx.OnLog?.Invoke((LogType.Warning, $"Clear audio {type}\n{ex.Message}"));
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.AudioPlaybackFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    $"Failed to play audio '{assetName}'.",
                    exception: ex));
            }
        }

        private AudioSource UpdateAudioSource(string assetName, AudioClip audioClip, Audio audioType)
        {
            if (!_audioSources.TryGetValue(audioType, out var audioSource))
            {
                var audioObject = new GameObject($"NovelAudio-{audioType}");
                audioSource = audioObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                _audioSources[audioType] = audioSource;
            }

            audioSource.Stop();
            if (audioType != Audio.Sound && audioSource.clip != null)
                GameObject.Destroy(audioSource.clip);
            audioSource.name = $"{assetName}-{audioType}";
            audioSource.clip = audioClip;
            switch (audioType)
            {
                case Audio.Music:
                    audioSource.loop = true;
                    audioSource.outputAudioMixerGroup = GetMixerGroup(_musicMixerGroup);
                    break;
                case Audio.Sound:
                    audioSource.loop = false;
                    audioSource.outputAudioMixerGroup = GetMixerGroup(_soundMixerGroup);
                    break;
                case Audio.Ambient:
                    audioSource.loop = true;
                    audioSource.outputAudioMixerGroup = GetMixerGroup(_ambientMixerGroup);
                    break;
            }
            audioSource.Play();
            return audioSource;
        }

        private AudioMixerGroup GetMixerGroup(string groupName)
        {
            if (_ctx.AudioMixer == null)
                throw new InvalidOperationException("AudioMixer is not configured.");

            var groups = _ctx.AudioMixer.FindMatchingGroups(groupName);
            if (groups.Length == 0)
                throw new InvalidOperationException(
                    $"AudioMixer group '{groupName}' is not configured.");

            return groups[0];
        }

        private void ClearAudio(Audio audioType)
        {
            if (_audioSources.TryGetValue(audioType, out var source))
            {
                source.Stop();
                source.clip = null;
            }
        }

        private static AudioType GetAudioType(string url)
        {
            return url.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                ? AudioType.MPEG
                : AudioType.WAV;
        }

        private void CacheSound(string assetName, AudioClip clip)
        {
            if (_soundClips.ContainsKey(assetName))
                return;
            _soundClips[assetName] = clip;
            _soundClipOrder.Enqueue(assetName);
            while (_soundClipOrder.Count > _soundCacheCapacity)
            {
                var oldest = _soundClipOrder.Dequeue();
                if (_soundClips.Remove(oldest, out var removed) && removed != null)
                    GameObject.Destroy(removed);
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var audioSource in _audioSources.Values)
            {
                if (audioSource != null)
                    GameObject.Destroy(audioSource.gameObject);
            }
            _audioSources.Clear();
            foreach (var clip in _soundClips.Values)
            {
                if (clip != null)
                    GameObject.Destroy(clip);
            }
            _soundClips.Clear();
            _soundClipOrder.Clear();
        }
    }
}
