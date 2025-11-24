using System;
using UnityEngine;

namespace Game.SomeMenu1
{
    [Serializable]
    public class Data
    {
        [SerializeField] private GameObject _someMenu1Prefab;

        public GameObject SomeMenu1Prefab => _someMenu1Prefab;
    }
}

