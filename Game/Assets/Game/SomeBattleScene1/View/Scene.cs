using System;
using UnityEngine;

namespace Game.SomeBattleScene1.View
{
    internal sealed class Scene : MonoBehaviour
    {
        internal struct Ctx
        {
            internal Func<float> GetPlayerInput;
            internal Action<int> OnComplete;
        }

        [SerializeField] private Animator _anim;

        [SerializeField] private Vector3 _camOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        [SerializeField] private Transform _targetPoint;

        private bool _sceneDone = false;

        private Ctx _ctx;

        internal void Setup(Ctx ctx)
        {
            _ctx = ctx;
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

            _anim.SetFloat("Move", _ctx.GetPlayerInput.Invoke());

            if (Vector3.Distance(_anim.rootPosition, _targetPoint.position) < 1)
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

