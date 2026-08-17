using System;

namespace Bundles
{
    public enum ContentSourceFailureKind
    {
        Unknown,
        Network,
        Timeout,
        NotFound,
        RateLimited,
        Server,
        Client,
    }

    public sealed class ContentSourceException : Exception
    {
        public ContentSourceException(string message, Exception inner = null)
            : base(message, inner)
        {
        }

        internal ContentSourceException(
            string message,
            ContentSourceFailureKind kind,
            long responseCode = 0,
            Exception inner = null)
            : base(message, inner)
        {
            Kind = kind;
            ResponseCode = responseCode;
        }

        public ContentSourceFailureKind Kind { get; }
        public long ResponseCode { get; }
        public bool IsTransient => Kind == ContentSourceFailureKind.Network
            || Kind == ContentSourceFailureKind.Timeout
            || Kind == ContentSourceFailureKind.RateLimited
            || Kind == ContentSourceFailureKind.Server;
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
