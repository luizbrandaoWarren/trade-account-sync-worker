using System;
using System.Runtime.Serialization;

namespace Warren.Trade.Risk.Infra.Exceptions
{
    [Serializable]
    public class CustodyNotUpdatedException : Exception
    {
        public CustodyNotUpdatedException()
        {
        }

        public CustodyNotUpdatedException(string message) : base(message)
        {
        }

        public CustodyNotUpdatedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CustodyNotUpdatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
