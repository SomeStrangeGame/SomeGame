using System;

namespace Bundles
{
    public sealed class ContentSourceException : Exception
    {
        public ContentSourceException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }

    public sealed class ContentIntegrityException : Exception
    {
        public ContentIntegrityException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }

    public sealed class ContentCompatibilityException : Exception
    {
        public ContentCompatibilityException(string message)
            : base(message)
        {
        }
    }

    public sealed class ContentConfigurationException : Exception
    {
        public ContentConfigurationException(string message)
            : base(message)
        {
        }

        public ContentConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public sealed class ContentStorageException : Exception
    {
        public ContentStorageException(string message)
            : base(message)
        {
        }
    }
}
