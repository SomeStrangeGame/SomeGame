using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeBattleScene1.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        private Action<float> _onSliderValueChanged;

        private void OnEnable()
        {
            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener(result => _onSliderValueChanged.Invoke(result));
        }

        public void Setup (Action<float> onSliderValueChanged)
        {
            _onSliderValueChanged = onSliderValueChanged;
        }

        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }

}

