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
            public Func<string, string> GetAudioURL;
            public Action<string> LoadAudioToDict;
            public AudioMixer AudioMixer;
            public CancellationToken CancellationToken;

            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;

        private readonly Dictionary<Audio, GameObject> _audioObjects;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _audioObjects = new ();
        }
        
        public async UniTask PlayAudio(string assetName, Audio type)
        {
            try
            {
                _ctx.LoadAudioToDict(assetName);
                var audioURL = _ctx.GetAudioURL(assetName);
                AudioClip audioClip = null;
                using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.WAV))
                {
                    await audioRequest.SendWebRequest().WithCancellation(_ctx.CancellationToken);
                    if (audioRequest.result != UnityWebRequest.Result.Success)
                    {
                        throw new InvalidOperationException(
                            $"Audio request failed [{audioRequest.responseCode}] {audioURL}: {audioRequest.error}");
                    }

                    audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
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
            }
        }

        private AudioSource UpdateAudioSource(string assetName, AudioClip audioClip, Audio audioType)
        {
            if (_audioObjects.TryGetValue(audioType, out var objAudio))
            {
                GameObject.Destroy(objAudio);
                _audioObjects.Remove(audioType);
            }

            var audioObject = new GameObject($"{assetName}-{audioType}");
            var audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;
            switch (audioType)
            {
                case Audio.Music:
                    audioSource.loop = true;
                    _audioObjects[audioType] = audioObject;
                    audioSource.outputAudioMixerGroup = GetMixerGroup(_musicMixerGroup);
                    break;
                case Audio.Sound:
                    audioSource.loop = false;
                    _audioObjects[audioType] = audioObject;
                    audioSource.outputAudioMixerGroup = GetMixerGroup(_soundMixerGroup);
                    break;
                case Audio.Ambient:
                    audioSource.loop = true;
                    _audioObjects[audioType] = audioObject;
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
            switch (audioType)
            {
                case Audio.Music:
                    if (_audioObjects.TryGetValue(audioType, out var objMusic))
                    {
                        GameObject.Destroy(objMusic);
                        _audioObjects.Remove(audioType);
                    }
                    break;
                case Audio.Sound:
                    if (_audioObjects.TryGetValue(audioType, out var objSound))
                    {
                        GameObject.Destroy(objSound);
                        _audioObjects.Remove(audioType);
                    }
                    break;
                case Audio.Ambient:
                    if (_audioObjects.TryGetValue(audioType, out var ambientSource))
                    {
                        GameObject.Destroy(ambientSource);
                        _audioObjects.Remove(audioType);
                    }
                    break;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            foreach (var audioObject in _audioObjects.Values)
            {
                if (audioObject != null)
                    GameObject.Destroy(audioObject);
            }
            _audioObjects.Clear();
        }
    }
}
