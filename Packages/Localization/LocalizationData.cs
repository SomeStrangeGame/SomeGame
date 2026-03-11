using System;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "ScriptableObjects/LocalizationSO")]
    public class LocalizationData : ScriptableObject
    {
        [Serializable]
        private struct Pair
        {
            [SerializeField]
            private string _key;
            [SerializeField]
            private string _value;

            internal readonly string Key => _key;
            internal readonly string Value => _value;

            public Pair(string key, string value)
            {
                _key = key;
                _value = value;
            } 
        }

        [SerializeField]
        private Pair[] _pairs;

        public bool TryGetValue(string key, out string value)
        {
            value = key;
            foreach(var pair in _pairs)
            {
                if (pair.Key != key) continue;

                value = pair.Value;
                return true;
            }

            return false;
        }
    }
}

