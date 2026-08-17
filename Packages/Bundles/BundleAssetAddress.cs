using System;

namespace Bundles
{
    public readonly struct BundleAssetAddress
    {
        public BundleAssetAddress(string bundleName, string assetName)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
                throw new ArgumentException("Bundle name must not be empty.", nameof(bundleName));
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentException("Asset name must not be empty.", nameof(assetName));

            BundleName = bundleName;
            AssetName = assetName;
        }

        public string BundleName { get; }
        public string AssetName { get; }

        public override string ToString() => $"{BundleName}:{AssetName}";
    }
}
