using System;
using UnityEngine;

namespace Game.Loading
{
    [Serializable]
    public class Data
    {
        [SerializeField] private GameObject _loadingPrefab;

        public GameObject LoadingPrefab => _loadingPrefab;
    }
}

