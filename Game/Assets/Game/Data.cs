using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class Data
    {
        [SerializeField] private Loading.Data _loadingData;

        public Loading.Data LoadingData => _loadingData;
    }
}

