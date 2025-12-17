using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ChapterScreen.View
{
    public sealed class Screen : MonoBehaviour
    {
        public struct Ctx
        {
            public Sprite BackgroundSprite;
            public string DescriptionText;
            public string ButtonText;
            public Action OnComplete;
        }

        [SerializeField] private float _startDelay = 3f;
        [SerializeField] private float _autoScrollSpeed = 25f;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TMPro.TMP_Text _descriptionTextArea;
        [SerializeField] private TMPro.TMP_Text _buttonTextArea;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private Button _someButton;

        private Ctx _ctx;

        private float _startTime;

        private void OnEnable()
        {
            _someButton.onClick.RemoveAllListeners();
            _someButton.onClick.AddListener(() => _ctx.OnComplete.Invoke());

            _startTime = 0f;
        }

        private void Update()
        {
            if (_startTime < _startDelay)
            {
                _startTime += Time.deltaTime;
                return;
            }

            if (SimpleInput.GetMouseButton(0)) return;
            
            _scroll.content.position += _autoScrollSpeed * Time.deltaTime * Vector3.up;
        }

        public void Setup(Ctx ctx)
        {
            _ctx = ctx;

            _backgroundImage.sprite = _ctx.BackgroundSprite;
            _descriptionTextArea.text = _ctx.DescriptionText;
            _buttonTextArea.text = _ctx.ButtonText;
        }

        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

