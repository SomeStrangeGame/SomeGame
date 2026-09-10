using System;
using UnityEngine;

namespace Novels
{
    public sealed class ContentRuntimeConfiguration : ScriptableObject
    {
        public const string AssetPath =
            "Assets/Resources/Novels/ContentRuntimeConfiguration.asset";
        private const string _resourcePath = "Novels/ContentRuntimeConfiguration";

        [SerializeField] private string _remoteContentBaseUrl;
        [SerializeField] private string _contentChannel = "dev";

        public string RemoteContentBaseUrl => _remoteContentBaseUrl;
        public string ContentChannel => _contentChannel;

        internal static ContentRuntimeConfiguration Load()
        {
            var configuration = Resources.Load<ContentRuntimeConfiguration>(
                _resourcePath);
            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Player content runtime configuration is missing.");
            }
            return configuration;
        }
    }
}
