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

        private readonly Dictionary<string, GameObject> _musicObjects;
        private readonly Dictionary<string, GameObject> _soundObjects;
        private readonly Dictionary<string, GameObject> _ambientObjects;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;

            _musicObjects = new ();
            _soundObjects = new ();
            _ambientObjects = new ();
        }
        
        public async UniTask PlayAudio(string assetName, Audio type)
        {
            _ctx.LoadAudioToDict(assetName);
            var audioURL = _ctx.GetAudioURL(assetName);
            AudioClip audioClip = null;
            using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(audioURL, AudioType.WAV))
            {
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
                        if (_musicObjects.TryGetValue(assetName, out var objMusic))
                        {
                            GameObject.Destroy(objMusic);
                            _musicObjects.Remove(assetName);
                        }
                        break;
                    case Audio.Sound:
                        if (_soundObjects.TryGetValue(assetName, out var objSound))
                        {
                            GameObject.Destroy(objSound);
                            _soundObjects.Remove(assetName);
                        }
                        break;
                    case Audio.Ambient:
                        if (_ambientObjects.TryGetValue(assetName, out var ambientSource))
                        {
                            GameObject.Destroy(ambientSource);
                            _ambientObjects.Remove(assetName);
                        }
                        break;
                }
                return null;
            }
            var audioObject = new GameObject(assetName);
            var audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;
            switch (audioType)
            {
                case Audio.Music:
                    audioSource.loop = true;
                    _musicObjects[assetName] = audioObject;
                    break;
                case Audio.Sound:
                    audioSource.loop = false;
                    _soundObjects[assetName] = audioObject;
                    break;
                case Audio.Ambient:
                    audioSource.loop = true;
                    _ambientObjects[assetName] = audioObject;
                    break;
            }
            audioSource.Play();
            return audioSource;
        }
    }
}

