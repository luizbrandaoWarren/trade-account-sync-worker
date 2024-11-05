using System;
using System.Runtime.Serialization;

namespace Warren.Trade.Risk.Infra.Exceptions
{
    [Serializable]
    public class NonTradeCustomerException : Exception
    {
        public NonTradeCustomerException()
        {
        }

        public NonTradeCustomerException(string message) : base(message)
        {
        }

        public NonTradeCustomerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NonTradeCustomerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
