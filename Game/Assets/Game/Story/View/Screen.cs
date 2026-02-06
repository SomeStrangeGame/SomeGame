using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Story.View
{
    public sealed class Screen : MonoBehaviour
    {
        public struct Ctx
        {
            public Sprite BackgroundSprite;
        }

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _descriptionTextArea;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Button _nextButton;

        private Ctx _ctx;

        public async UniTask ShowText(string text)
        {
            var token = new UniTaskCompletionSource();
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(() => token.TrySetResult());

            var le = _backgroundImage.GetComponent<LayoutElement>();
            le.preferredWidth = _scroll.content.rect.width;

            _descriptionTextArea.text = text.Replace("<br/>", System.Environment.NewLine);
            await token.Task;
        }

        public void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _backgroundImage.sprite = _ctx.BackgroundSprite;
            var aspectRatio = (float)_ctx.BackgroundSprite.texture.width / _ctx.BackgroundSprite.texture.height;
            var le = _backgroundImage.GetComponent<LayoutElement>();
            le.preferredHeight = _scroll.content.rect.width / aspectRatio;
            _descriptionTextArea.text = string.Empty;
        }

        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

