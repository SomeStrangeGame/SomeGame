using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeMenu1.View
{
    public class Screen : MonoBehaviour
    {
        [SerializeField] private Button _someButton;

        private Action<int> _onComplete;

        private void OnEnable()
        {
            _someButton.onClick.RemoveAllListeners();
            _someButton.onClick.AddListener(() => _onComplete.Invoke(1));
        }

        public void Setup(Action<int> onComplete)
        {
            _onComplete = onComplete;
        }

        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

