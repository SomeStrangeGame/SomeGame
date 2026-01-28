using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "ScreenData", menuName = "ScriptableObjects/ScreenData")]
    public class ScreenData : ScriptableObject
    {
        [SerializeField] private BundleData _menuBundle;
        [SerializeField] private BundleData _backgroundBundle;

        [SerializeField] private string _buttonText;
        [SerializeField][TextArea(15, 250)] private string _descriptionText;

        public BundleData MenuBundle => _menuBundle;
        public BundleData BackgroundBundle => _backgroundBundle;

        public string ButtonText => _buttonText;
        public string DescriptionText => _descriptionText;
    }
}

