using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "BattleData", menuName = "ScriptableObjects/BattleData")]
    public class BattleData : ScriptableObject
    {
        [SerializeField] private BundleData _characterBundle;
        [SerializeField] private BundleData _sceneBundle;
        [SerializeField] private BundleData _screenBundle;
        [SerializeField] private BundleData _cameraBundle;

        public BundleData CharacterBundle => _characterBundle;
        public BundleData SceneBundle => _sceneBundle;
        public BundleData ScreenBundle => _screenBundle;
        public BundleData CameraBundle => _cameraBundle;
    }
}

