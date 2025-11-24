using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class Data
    {
        [SerializeField] private Loading.Data _loadingData;
        [SerializeField] private SomeMenu1.Data _someMenu1Data;

        public Loading.Data LoadingData => _loadingData;
        public SomeMenu1.Data SomeMenu1Data => _someMenu1Data;
    }
}

