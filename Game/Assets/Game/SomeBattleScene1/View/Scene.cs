using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.SomeBattleScene1.View
{
    public interface ISomeBattleScene1
    {
        public void Release();
        public UniTask<int> GetProcess();
        public void InitScreen(ISomeBattleScene1Screen screen);
    }
    
    public class Scene : MonoBehaviour, ISomeBattleScene1
    {
        [SerializeField] private Animator _anim;

        [SerializeField] private Vector3 _camOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        [SerializeField] private Transform _targetPoint;

        private readonly UniTaskCompletionSource<int> _someToken = new();

        private float _inputValue;
        private bool _sceneDone = false;

        public void InitScreen(ISomeBattleScene1Screen screen)
        {
            screen.SliderEvent.RemoveAllListeners();
            screen.SliderEvent.AddListener(value => _inputValue = value);
        }

        private void Update()
        {
            if (_sceneDone) return;

            var cameraTrans = Camera.allCameras[0].transform;
            var cameraTarget = _anim.rootPosition + _camOffset;
            cameraTrans.position = Vector3.Lerp(cameraTrans.position, cameraTarget, Time.deltaTime * _camMoveSpeed);

            var cameraLookAtTarget = _anim.rootPosition + _camLookAtOffset;
            var camRot = cameraTrans.rotation;
            cameraTrans.LookAt(cameraLookAtTarget);
            cameraTrans.rotation = Quaternion.Lerp(camRot, cameraTrans.rotation, Time.deltaTime * _camLookAtSpeed);

            _anim.SetFloat("Move", _inputValue);

            if (Vector3.Distance(_anim.rootPosition, _targetPoint.position) < 1)
            {
                _sceneDone = true;
                _someToken.TrySetResult(2);
            }
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

