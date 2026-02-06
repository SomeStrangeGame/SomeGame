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
        }

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _descriptionTextArea;
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private Button _nextButton;
        [SerializeField] private float _showHideDuration;
        [SerializeField] private CanvasGroup _canvasGroup;

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

        public async UniTask ShowText(string text)
        {
            var token = new UniTaskCompletionSource();
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(() => token.TrySetResult());

            var le = _descriptionTextArea.GetComponent<LayoutElement>();
            le.preferredWidth = _viewportRect.rect.width;

            _descriptionTextArea.text = text.Replace("<br/>", System.Environment.NewLine);

            await ShowingText();
            await token.Task;
            await HidingText();
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
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

