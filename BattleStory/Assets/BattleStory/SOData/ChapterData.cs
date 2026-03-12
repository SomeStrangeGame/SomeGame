using UnityEngine;

namespace BattleStory.SOData
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "ScriptableObjects/ChapterData")]
    public class ChapterData : ScriptableObject
    {
        [SerializeField] private ScreenData[] _introMenu;
        [SerializeField] private ScreenData[] _startMenu;
        [SerializeField] private ScreenData[] _successMenu;
        [SerializeField] private ScreenData[] _failedMenu;

        [SerializeField] private BattleData[] _battles;

        public ScreenData[] IntroMenu => _introMenu;
        public ScreenData[] StartMenu => _startMenu;
        public ScreenData[] SuccessMenu => _successMenu;
        public ScreenData[] FailedMenu => _failedMenu;

        public BattleData[] Battles => _battles;
    }
}
