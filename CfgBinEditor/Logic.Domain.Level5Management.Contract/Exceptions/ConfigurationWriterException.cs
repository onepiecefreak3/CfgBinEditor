using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.Contract.Exceptions
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
