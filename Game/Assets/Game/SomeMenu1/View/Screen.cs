using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeMenu1.View
{
    internal sealed class Screen : MonoBehaviour
    {
        internal struct Ctx
        {
            internal Action<int> OnComplete;
        }

        [SerializeField] private Button _someButton;

        private Ctx _ctx;

        private void OnEnable()
        {
            _someButton.onClick.RemoveAllListeners();
            _someButton.onClick.AddListener(() => _ctx.OnComplete.Invoke(1));
        }

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

