using System;
using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "BattleData", menuName = "ScriptableObjects/BattleData")]
    public class BattleData : ScriptableObject
    {
        [Serializable]
        public struct CameraData
        {
            [SerializeField] private Vector3 _camMoveOffset;
            [SerializeField] private float _camMoveSpeed;
            [SerializeField] private Vector3 _camLookAtOffset;
            [SerializeField] private float _camLookAtSpeed;

            public readonly Vector3 CamMoveOffset => _camMoveOffset;
            public readonly float CamMoveSpeed => _camMoveSpeed;
            public readonly Vector3 CamLookAtOffset => _camLookAtOffset;
            public readonly float CamLookAtSpeed => _camLookAtSpeed;
        }

        [SerializeField] private BundleData _characterBundle;
        [SerializeField] private BundleData _sceneBundle;
        [SerializeField] private BundleData _screenBundle;
        [SerializeField] private CameraData _camera;

        public BundleData CharacterBundle => _characterBundle;
        public BundleData SceneBundle => _sceneBundle;
        public BundleData ScreenBundle => _screenBundle;
        public CameraData Camera => _camera;
    }
}

