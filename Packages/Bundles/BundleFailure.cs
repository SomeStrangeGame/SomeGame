using System;

namespace Bundles
{
    public readonly struct BundleFailure
    {
        public BundleFailure(string code, string message, Exception exception = null)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Exception = exception;
        }

        public string Code { get; }
        public string Message { get; }
        public Exception Exception { get; }
    }

    public static class BundleFailureCodes
    {
        public const string InvalidBundleName = "INVALID_BUNDLE_NAME";
        public const string AssetNotFound = "BUNDLE_ASSET_NOT_FOUND";
    }
}
