using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.SomeMenu1.View
{
    public interface ISomeScreen1 
    {
        public void Release();
        public UniTask<int> GetProcess();
    }
    
    public class Screen : MonoBehaviour, ISomeScreen1
    {
        [SerializeField] private Button _someButton;

        private readonly UniTaskCompletionSource<int> _someToken = new();

        private void OnEnable()
        {
            _someButton.onClick.RemoveAllListeners();
            _someButton.onClick.AddListener(() => _someToken.TrySetResult(1));
        }

        public async UniTask<int> GetProcess()
        {
            return await _someToken.Task;
        }

        public void Release() 
        {
            if (this != null)
                GameObject.Destroy(gameObject);
        }
    }
}

