using System;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels.Audio
{
    public class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Func<string, string> GetAudioURL;
            public Action<string> LoadAudioToDict;

            public Action<(LogType type, string message)> OnLog;
        }

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }
        
        public async UniTask PlayAudio(string assetName)
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
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }

        private void SetHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
    }
}

