using UnityEngine;

namespace Novels.Location
{
    [CreateAssetMenu(fileName = "VideosSO", menuName = "ScriptableObjects/VideosSO")]
    public class VideosSO : ScriptableObject
    {
        [SerializeField] private string[] _videos;

        public string[] Videos => _videos;
    }
}

