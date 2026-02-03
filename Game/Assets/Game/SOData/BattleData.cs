using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "BattleData", menuName = "ScriptableObjects/BattleData")]
    public class BattleData : ScriptableObject
    {
        [SerializeField] private BundleData _characterBundle;
        [SerializeField] private BundleData _characterScreenBundle;
        [SerializeField] private BundleData _sceneBundle;
        [SerializeField] private BundleData _screenBundle;
        [SerializeField] private CameraData _camera;

        public BundleData CharacterBundle => _characterBundle;
        public BundleData CharacterScreenBundle => _characterScreenBundle;
        public BundleData SceneBundle => _sceneBundle;
        public BundleData ScreenBundle => _screenBundle;
        public CameraData Camera => _camera;
    }
}

