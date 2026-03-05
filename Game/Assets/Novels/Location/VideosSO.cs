using UnityEngine;

namespace Novels.Location
{
    [CreateAssetMenu(fileName = "VideosSO", menuName = "ScriptableObjects/VideosSO")]
    public class VideosSO : ScriptableObject
    {
        [SerializeField] private string[] _videos;
        [SerializeField] private string[] _cutScenes;

        public string[] Videos => _videos;
        public string[] CutScenes => _cutScenes;
    }
}

