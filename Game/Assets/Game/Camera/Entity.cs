using System;
using Cysharp.Threading.Tasks;
using Game.Disposable;
using UnityEngine;

namespace Game.Camera
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public Vector3 MoveOffset;
            public float MoveSpeed;
            public Vector3 LookAtOffset;
            public float LookAtSpeed;

            public Func<Vector3> GetCameraTargetPosition;
        }

        private Transform _cameraTrans;

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public async UniTask Init()
        {
            _cameraTrans = UnityEngine.Camera.allCameras[0].transform;
        }

        public void UpdatePos(float deltaTime)
        {
            var cameraTarget = _ctx.GetCameraTargetPosition.Invoke() + _ctx.MoveOffset;
            _cameraTrans.position = Vector3.Lerp(_cameraTrans.position, cameraTarget, deltaTime * _ctx.MoveSpeed);

            var cameraLookAtTarget = _ctx.GetCameraTargetPosition.Invoke()  + _ctx.LookAtOffset;
            var camRot = _cameraTrans.rotation;
            _cameraTrans.LookAt(cameraLookAtTarget);
            _cameraTrans.rotation = Quaternion.Lerp(camRot, _cameraTrans.rotation, deltaTime * _ctx.LookAtSpeed);
        }
    }
}

