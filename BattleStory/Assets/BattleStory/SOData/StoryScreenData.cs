using SOData;
using UnityEngine;

namespace BattleStory.SOData
{
    [CreateAssetMenu(fileName = "StoryScreenData", menuName = "ScriptableObjects/StoryScreenData")]
    public class StoryScreenData : ScriptableObject
    {
        [SerializeField] private BundleData _screenBundle;

        public BundleData ScreenBundle => _screenBundle;
    }
}

