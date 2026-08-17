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
}
