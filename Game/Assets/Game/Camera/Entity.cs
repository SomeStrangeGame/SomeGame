using System;
using Game.Disposable;
using Game.SOData;
using UnityEngine;

namespace Game.Camera
{
    public sealed class Entity : BaseDisposable
    {
        public struct Ctx
        {
            public CameraData Data;

            public Func<Vector3> GetCameraTargetPosition;
        }

        private Transform _cameraTrans;

        private readonly Ctx _ctx;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            _cameraTrans = UnityEngine.Camera.allCameras[0].transform;
        }

        public void UpdatePos(float deltaTime)
        {
            var cameraTarget = _ctx.GetCameraTargetPosition.Invoke() + _ctx.Data.CamMoveOffset;
            _cameraTrans.position = Vector3.Lerp(_cameraTrans.position, cameraTarget, deltaTime * _ctx.Data.CamMoveSpeed);

            var cameraLookAtTarget = _ctx.GetCameraTargetPosition.Invoke()  + _ctx.Data.CamLookAtOffset;
            var camRot = _cameraTrans.rotation;
            _cameraTrans.LookAt(cameraLookAtTarget);
            _cameraTrans.rotation = Quaternion.Lerp(camRot, _cameraTrans.rotation, deltaTime * _ctx.Data.CamLookAtSpeed);
        }
    }
}

