using UnityEngine;

namespace Game.Chapter_ScreenAndBattle
{
    [CreateAssetMenu(fileName = "CameraData", menuName = "ScriptableObjects/CameraData")]
    public class CameraDataSO : ScriptableObject
    {
        [SerializeField] private Vector3 _camMoveOffset;
        [SerializeField] private float _camMoveSpeed;
        [SerializeField] private Vector3 _camLookAtOffset;
        [SerializeField] private float _camLookAtSpeed;

        internal Vector3 CamMoveOffset => _camMoveOffset;
        internal float CamMoveSpeed => _camMoveSpeed;
        internal Vector3 CamLookAtOffset => _camLookAtOffset;
        internal float CamLookAtSpeed => _camLookAtSpeed;
    }
}

