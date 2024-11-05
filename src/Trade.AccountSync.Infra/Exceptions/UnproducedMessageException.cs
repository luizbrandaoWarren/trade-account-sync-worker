using System;
using System.Runtime.Serialization;

namespace Warren.Trade.Risk.Infra.Exceptions
{
    [Serializable]
    public class UnproducedMessageException : Exception
    {
        public UnproducedMessageException()
        {
        }

        public UnproducedMessageException(string message) : base(message)
        {
        }

        public UnproducedMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnproducedMessageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
