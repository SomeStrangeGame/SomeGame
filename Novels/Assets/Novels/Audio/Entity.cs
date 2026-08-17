using System;
using System.Collections.Generic;
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
        private const int _soundVoiceCount = 4;
        private readonly Dictionary<Audio, AudioSource> _loopSources;
        private readonly List<AudioSource> _soundVoices = new();
        private readonly Dictionary<string, AudioClip> _soundClips = new(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> _soundClipOrder = new();

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _loopSources = new();
        }
        
        public async UniTask PlayAudio(string assetName, Audio type)
        {
            try
            {
                var audioURL = await _ctx.ResolveAudioUrl(assetName);
                if (string.IsNullOrEmpty(audioURL))
                {
                    ClearAudio(type);
                    _ctx.OnLog?.Invoke((LogType.Log, $"Stop audio {type}"));
                    return;
                }

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
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.AudioPlaybackFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    $"Failed to play audio '{assetName}'.",
                    exception: ex));
            }
        }

        private AudioSource UpdateAudioSource(string assetName, AudioClip audioClip, Audio audioType)
        {
            var audioSource = audioType == Audio.Sound
                ? GetSoundVoice()
                : GetLoopSource(audioType);

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

        private AudioSource GetLoopSource(Audio audioType)
        {
            if (_loopSources.TryGetValue(audioType, out var source))
                return source;

            source = CreateAudioSource($"NovelAudio-{audioType}");
            _loopSources[audioType] = source;
            return source;
        }

        private AudioSource GetSoundVoice()
        {
            foreach (var voice in _soundVoices)
            {
                if (!voice.isPlaying)
                    return voice;
            }

            if (_soundVoices.Count < _soundVoiceCount)
            {
                var voice = CreateAudioSource($"NovelAudio-Sound-{_soundVoices.Count}");
                _soundVoices.Add(voice);
                return voice;
            }

            return _soundVoices[0];
        }

        private static AudioSource CreateAudioSource(string name)
        {
            var audioObject = new GameObject(name);
            var source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
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
            if (audioType == Audio.Sound)
            {
                foreach (var voice in _soundVoices)
                {
                    voice.Stop();
                    voice.clip = null;
                }
                return;
            }

            if (_loopSources.TryGetValue(audioType, out var source))
            {
                source.Stop();
                var clip = source.clip;
                source.clip = null;
                if (clip != null)
                    GameObject.Destroy(clip);
            }
        }

        private static AudioType GetAudioType(string url)
        {
            if (url.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                return AudioType.MPEG;
            if (url.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                return AudioType.OGGVORBIS;
            return AudioType.WAV;
        }

        private void CacheSound(string assetName, AudioClip clip)
        {
            if (_soundClips.ContainsKey(assetName))
                return;
            _soundClips[assetName] = clip;
            _soundClipOrder.Enqueue(assetName);
            var attempts = _soundClipOrder.Count;
            while (_soundClips.Count > _soundCacheCapacity && attempts-- > 0)
            {
                var oldest = _soundClipOrder.Dequeue();
                if (!_soundClips.TryGetValue(oldest, out var removed))
                    continue;
                if (IsClipInUse(removed))
                {
                    _soundClipOrder.Enqueue(oldest);
                    continue;
                }
                _soundClips.Remove(oldest);
                if (removed != null)
                    GameObject.Destroy(removed);
            }
        }

        private bool IsClipInUse(AudioClip clip)
        {
            foreach (var voice in _soundVoices)
            {
                if (voice.isPlaying && voice.clip == clip)
                    return true;
            }
            return false;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var audioSource in _loopSources.Values)
            {
                if (audioSource != null)
                {
                    if (audioSource.clip != null)
                        GameObject.Destroy(audioSource.clip);
                    GameObject.Destroy(audioSource.gameObject);
                }
            }
            _loopSources.Clear();
            foreach (var voice in _soundVoices)
            {
                if (voice != null)
                    GameObject.Destroy(voice.gameObject);
            }
            _soundVoices.Clear();
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
