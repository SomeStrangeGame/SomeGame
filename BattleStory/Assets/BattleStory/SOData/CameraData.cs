using UnityEngine;

namespace BattleStory.SOData
{
    [CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraData")]
    public class CameraData : ScriptableObject
    {
        [SerializeField] private Vector3 _camMoveOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        public Vector3 CamMoveOffset => _camMoveOffset;
        public float CamMoveSpeed => _camMoveSpeed;
        public Vector3 CamLookAtOffset => _camLookAtOffset;
        public float CamLookAtSpeed => _camLookAtSpeed;
    }
}

