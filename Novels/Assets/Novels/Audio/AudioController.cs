using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;
using UnityEngine.Audio;

namespace Novels.Audio
{
    public class AudioController : BaseDisposable
    {
        private const int _soundCacheCapacity = 8;
        private const int _soundVoiceCount = 4;

        public enum Audio
        {
            Music,
            Sound,
            Ambient
        }

        public struct Dependencies
        {
            public Func<string, UniTask<string>> ResolveAudioUrl;
            public AudioMixer AudioMixer;
            public CancellationToken CancellationToken;
            public Action<(LogType type, string message)> OnLog;
            public Action<Diagnostics.NovelError> OnError;
        }

        private readonly Dependencies _ctx;
        private readonly AudioClipLoader _clipLoader;
        private readonly LoopAudioChannels _loopChannels;
        private readonly SoundVoicePool _soundVoices;

        public AudioController(Dependencies ctx)
        {
            _ctx = ctx;
            if (ctx.ResolveAudioUrl == null)
                throw new ArgumentNullException(nameof(ctx.ResolveAudioUrl));
            var mixerGroups = new AudioMixerGroups(ctx.AudioMixer);
            _clipLoader = new AudioClipLoader(ctx.CancellationToken);
            _loopChannels = new LoopAudioChannels(mixerGroups);
            _soundVoices = new SoundVoicePool(
                _soundVoiceCount,
                _soundCacheCapacity,
                mixerGroups);
        }

        public async UniTask PlayAudio(string assetName, Audio type)
        {
            try
            {
                var url = await _ctx.ResolveAudioUrl(assetName);
                if (string.IsNullOrEmpty(url))
                {
                    ClearAudio(type);
                    _ctx.OnLog?.Invoke((LogType.Log, $"Stop audio {type}"));
                    return;
                }

                if (type == Audio.Sound)
                {
                    if (!_soundVoices.TryGetClip(assetName, out var clip))
                        clip = await _clipLoader.Load(url, false);
                    _soundVoices.Play(assetName, clip);
                }
                else
                {
                    var clip = await _clipLoader.Load(url, true);
                    _loopChannels.Play(assetName, clip, type);
                }
                _ctx.OnLog?.Invoke((LogType.Log, $"Play audio {assetName}"));
            }
            catch (OperationCanceledException) when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                ClearAudio(type);
                _ctx.OnError?.Invoke(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.AudioPlaybackFailed,
                    Diagnostics.NovelErrorSeverity.Recoverable,
                    $"Failed to play audio '{assetName}'.",
                    exception: exception));
            }
        }

        private void ClearAudio(Audio type)
        {
            if (type == Audio.Sound)
                _soundVoices.Clear();
            else
                _loopChannels.Clear(type);
        }

        protected override void OnDispose()
        {
            _loopChannels.Dispose();
            _soundVoices.Dispose();
            base.OnDispose();
        }
    }
}
