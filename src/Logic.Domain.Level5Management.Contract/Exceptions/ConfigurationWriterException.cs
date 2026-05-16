using System.Runtime.Serialization;

namespace Logic.Domain.Level5Management.Contract.Exceptions
{
    [Serializable]
    public class ConfigurationWriterException : Exception
    {
        public ConfigurationWriterException()
        {
        }

        public ConfigurationWriterException(string message) : base(message)
        {
        }

        public ConfigurationWriterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationWriterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
