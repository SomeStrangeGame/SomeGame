using System;
using UnityEngine;

namespace Game.SomeBattleScene1.View
{
    public class Scene : MonoBehaviour
    {
        [SerializeField] private Animator _anim;

        [SerializeField] private Vector3 _camOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        [SerializeField] private Transform _targetPoint;

        private bool _sceneDone = false;

        private Func<float> _getPlayerInput;
        private Action<int> _onComplete;

        public void Setup(Func<float> getPlayerInput, Action<int> onComplete)
        {
            _getPlayerInput = getPlayerInput;
            _onComplete = onComplete;
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

            _anim.SetFloat("Move", _getPlayerInput.Invoke());

            if (Vector3.Distance(_anim.rootPosition, _targetPoint.position) < 1)
            {
                _sceneDone = true;
                _onComplete.Invoke(2);
            }
        }

        public void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

