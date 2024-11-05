using System;
using System.Runtime.Serialization;

namespace Warren.Trade.Risk.Infra.Exceptions
{
    [Serializable]
    public class CacheInvalidationException : Exception
    {
        public CacheInvalidationException()
        {
        }

        public CacheInvalidationException(string message) : base(message)
        {
        }

        public CacheInvalidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CacheInvalidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
