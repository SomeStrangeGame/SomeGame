using System;
using UnityEngine;

namespace SOData
{
    [Serializable]
    public struct BundleData
    {
        [SerializeField] private string _bundleName;
        [SerializeField] private string _assetName;

        public readonly string BundleName => _bundleName;
        public readonly string AssetName => _assetName;
    }
}

