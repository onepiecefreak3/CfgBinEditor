using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
