using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeBattleScene1.View
{
    internal sealed class Screen : MonoBehaviour
    {
        internal struct Ctx
        {
            internal Action<float> OnSliderValueChanged;
        }

        [SerializeField] private Slider _slider;

        private Ctx _ctx;

        private void OnEnable()
        {
            _slider.onValueChanged.RemoveAllListeners();
            _slider.onValueChanged.AddListener(result => _ctx.OnSliderValueChanged.Invoke(result));
        }

        internal void Setup (Ctx ctx)
        {
            _ctx = ctx;
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }

}

