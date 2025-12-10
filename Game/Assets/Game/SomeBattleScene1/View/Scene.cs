using System;
using UnityEngine;

namespace Game.SomeBattleScene1.View
{
    internal sealed class Scene : MonoBehaviour
    {
        internal struct Ctx
        {
            internal Action<int> OnComplete;
        }

        [SerializeField] private GameObject _playerCharacter;
        [SerializeField] private GameObject _targetObject;

        [SerializeField] private Vector3 _camOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        private bool _sceneDone = false;

        private Ctx _ctx;

        internal GameObject PlayerCharacter => _playerCharacter;
        internal GameObject TargetObject => _targetObject;

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;
        }

        private void Update()
        {
            if (_sceneDone) return;

            var cameraTrans = Camera.allCameras[0].transform;
            var cameraTarget = _playerCharacter.transform.position + _camOffset;
            cameraTrans.position = Vector3.Lerp(cameraTrans.position, cameraTarget, Time.deltaTime * _camMoveSpeed);

            var cameraLookAtTarget = _playerCharacter.transform.position + _camLookAtOffset;
            var camRot = cameraTrans.rotation;
            cameraTrans.LookAt(cameraLookAtTarget);
            cameraTrans.rotation = Quaternion.Lerp(camRot, cameraTrans.rotation, Time.deltaTime * _camLookAtSpeed);

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _sceneDone = true;
                _ctx.OnComplete.Invoke(2);
            }
        }

        internal void Release() 
        {
            if (this != null) GameObject.Destroy(gameObject);
        }
    }
}

