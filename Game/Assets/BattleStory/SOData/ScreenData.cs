using System;
using UnityEngine;

namespace Game.SOData
{
    [CreateAssetMenu(fileName = "ScreenData", menuName = "ScriptableObjects/ScreenData")]
    public class ScreenData : ScriptableObject
    {
        [SerializeField] private string _textAssetName;
        [SerializeField] private StoryScreenData _screenBundle;
        [SerializeField] private BundleData _backgroundBundle;

        public string TextAssetName => _textAssetName;
        public StoryScreenData ScreenBundle => _screenBundle;
        public BundleData BackgroundBundle => _backgroundBundle;
    }
}

