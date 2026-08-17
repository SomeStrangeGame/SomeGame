using System;

namespace Bundles
{
    [Serializable]
    public sealed class BundleManifest
    {
        public string version;
        public long size;
        public string sha256;
        public uint crc;

        public bool HasIntegrity =>
            size > 0 && !string.IsNullOrWhiteSpace(sha256);

        public static BundleManifest Legacy(string version)
        {
            return new BundleManifest { version = version };
        }
    }
}
