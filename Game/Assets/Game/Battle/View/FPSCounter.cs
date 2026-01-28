using UnityEngine;
using UnityEngine.UI;

namespace Game.Battle.View
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] private float _expSmoothingFactor = 0.9f;
        [SerializeField] private float _refreshFrequency = 0.4f;

        private float _timeSinceUpdate = 0f;
        private float _averageFps = 1f;

        private Text _text;
        private Text Text
        {
            get
            {
                if (_text == null) _text = GetComponentInChildren<Text>(true);
                return _text;
            }
        }

        private void Update()
        {
            // Exponentially weighted moving average (EWMA)
            _averageFps = _expSmoothingFactor * _averageFps + (1f - _expSmoothingFactor) * 1f / Time.unscaledDeltaTime;

            if (_timeSinceUpdate < _refreshFrequency)
            {
                _timeSinceUpdate += Time.deltaTime;
                return;
            }

            var fps = Mathf.RoundToInt(_averageFps);
            Text.text = fps.ToString();

            _timeSinceUpdate = 0f;
        }
    }
}

