using System.Runtime.Serialization;

namespace Logic.Domain.Level5Management.Contract.Exceptions
{
    [Serializable]
    public class ConfigurationReaderException : Exception
    {
        public ConfigurationReaderException()
        {
        }

        public ConfigurationReaderException(string message) : base(message)
        {
        }

        public ConfigurationReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
