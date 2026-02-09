using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BattleStory.Story.View
{
    public sealed class Screen : MonoBehaviour
    {
        public struct Ctx
        {
            public Sprite BackgroundSprite;
            public Func<string, UniTask<AudioClip>> GetAudioClip;
        }

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _descriptionTextArea;
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private Button _nextButton;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly Queue<AudioClip> _voices = new();
        private AudioSource _voicesSource;
        private AudioSource VoicesSource
        {
            get
            {
                if (_voicesSource == null) 
                {
                    _voicesSource = gameObject.AddComponent<AudioSource>();
                    _voicesSource.loop = false;
                    _voicesSource.playOnAwake = false;
                }
                return _voicesSource;
            }
        }

        private Ctx _ctx;

        public async UniTask ShowingText()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = 1f - (timer / _showHideDuration);
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 1f;
        }

        private void SetVoice(params AudioClip[] voices)
        {
            StopVoice();
            TryPlayVoice(voices).Forget();
        }

        private void StopVoice()
        {
            _voices.Clear();

            VoicesSource.Stop();
            VoicesSource.clip = null;
        }

        private async UniTask TryPlayVoice(params AudioClip[] voices)
        {
            foreach(var voice in voices)
                _voices.Enqueue(voice);

            if (!_voices.TryDequeue(out var currentVoice)) return;
            if (VoicesSource.isPlaying) return;

            VoicesSource.clip = currentVoice;
            VoicesSource.Play();

            while(VoicesSource.isPlaying)
                await UniTask.NextFrame();

            TryPlayVoice().Forget();
        }

        public async UniTask HidingText()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.gameObject.SetActive(true);

            var delayMs = 50;
            var deltaTime = delayMs / 1000f;

            var timer = _showHideDuration;
            while (timer >= 0f)
            {
                _canvasGroup.alpha = timer / _showHideDuration;
                timer -= deltaTime;
                await UniTask.Delay(delayMs, true);
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async UniTask TryProcessText(string text)
        {
            var token = new UniTaskCompletionSource();
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(() => token.TrySetResult());

            var le = _descriptionTextArea.GetComponent<LayoutElement>();
            le.preferredWidth = _viewportRect.rect.width;

            if (text.Contains("voices:"))
            {
                var voicesNames = text.Replace("voices:", string.Empty).Split(",");
                var voices = await voicesNames.Select(async v => await _ctx.GetAudioClip(v.Trim())).ToArray();
                SetVoice(voices);
                return;
            }
            
            _descriptionTextArea.text = text.Replace("<br/>", System.Environment.NewLine);
            await ShowingText();
            await token.Task;
            await HidingText();
            SetVoice();
        }

        public void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _backgroundImage.sprite = _ctx.BackgroundSprite;
            var aspectRatio = (float)_ctx.BackgroundSprite.texture.width / _ctx.BackgroundSprite.texture.height;
            var le = _backgroundImage.GetComponent<LayoutElement>();
            le.preferredHeight = _viewportRect.rect.width / aspectRatio;
            _descriptionTextArea.text = string.Empty;
        }

        public void Release() 
        {
            StopVoice();
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

