using UnityEngine;

namespace BattleStory.SOData
{
    [CreateAssetMenu(fileName = "BattleData", menuName = "ScriptableObjects/BattleData")]
    public class BattleData : ScriptableObject
    {
        [SerializeField] private BundleData _meleeCharacterBundle;
        [SerializeField] private BundleData _meleeCharacterScreenBundle;
        [SerializeField] private BundleData _distanceCharacterBundle;
        [SerializeField] private BundleData _distanceChracterScreenBundle;
        [SerializeField] private BundleData _sceneBundle;
        [SerializeField] private BundleData _screenBundle;
        [SerializeField] private CameraData _camera;

        public BundleData MeleeCharacterBundle => _meleeCharacterBundle;
        public BundleData MeleeCharacterScreenBundle => _meleeCharacterScreenBundle;
        public BundleData DistanceCharacterBundle => _distanceCharacterBundle;
        public BundleData DistanceCharacterScreenBundle => _distanceChracterScreenBundle;
        public BundleData SceneBundle => _sceneBundle;
        public BundleData ScreenBundle => _screenBundle;
        public CameraData Camera => _camera;
    }
}

