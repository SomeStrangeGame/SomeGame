using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "ChaptersData", menuName = "ScriptableObjects/ChaptersData")]
    public class ChaptersData : ScriptableObject
    {
        [SerializeField] private ChapterData[] _chapters;

        public ChapterData[] Chapters => _chapters;
    }
}
