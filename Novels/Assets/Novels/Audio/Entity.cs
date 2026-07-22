using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels.Audio
{
    public class Entity : BaseDisposable
    {
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
            _ctx.LoadAudioToDict(assetName);
            var audioURL = _ctx.GetAudioURL(assetName);
            AudioClip audioClip = null;
            using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.WAV))
            {
                Debug.Log($"Play audio {assetName}");
                SetHeaders(audioRequest);
                await audioRequest.SendWebRequest();

                audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
            }
            UpdateAudioSource(assetName, audioClip, type);
            _ctx.OnLog?.Invoke((LogType.Log, $"Play audio {assetName}"));
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }

        private AudioSource UpdateAudioSource(string assetName, AudioClip audioClip, Audio audioType)
        {
            if (assetName == "тишина")
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
                return null;
            }
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
                    break;
                case Audio.Sound:
                    audioSource.loop = false;
                    _audioObjects[audioType] = audioObject;
                    break;
                case Audio.Ambient:
                    audioSource.loop = true;
                    _audioObjects[audioType] = audioObject;
                    break;
            }
            audioSource.Play();
            return audioSource;
        }
    }
}

