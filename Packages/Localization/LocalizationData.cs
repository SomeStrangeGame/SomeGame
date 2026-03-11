using System;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(fileName = "LocalizationData", menuName = "ScriptableObjects/LocalizationData")]
    public class LocalizationData : ScriptableObject
    {
        public enum Language
        {
            Rus,
        }

        [Serializable]
        private struct Pair
        {
            [Serializable]
            private struct Languages
            {
                [SerializeField]
                private string _value;
                [SerializeField]
                private Language _language;

                internal readonly string Value => _value;
                internal readonly Language Language => _language;
            }

            [SerializeField]
            private string _key;
            [SerializeField]
            private Languages[] _languages;

            internal string Key => _key;

            internal bool TryGetValue(Language language, out string value)
            {
                value = string.Empty;
                foreach (var pair in _languages)
                {
                    if (pair.Language != language) continue;
                    value = pair.Value;
                    return true;
                }

                return false;
            }
        }

        [SerializeField]
        private Pair[] _pairs;

        public bool TryGetValue(Language language, string key, out string value)
        {
            value = string.Empty;
            foreach(var pair in _pairs)
            {
                if (pair.Key != key) continue;

                if (!pair.TryGetValue(language, out value))
                    return false;
                    
                return true;
            }

            return false;
        }
    }
}

