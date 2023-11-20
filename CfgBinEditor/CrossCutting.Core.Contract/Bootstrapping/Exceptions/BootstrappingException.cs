using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CrossCutting.Core.Contract.Bootstrapping.Exceptions
{
    public class BootstrappingException : Exception
    {
        public BootstrappingException()
        {
        }

        public BootstrappingException(string message) : base(message)
        {
        }

        public BootstrappingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BootstrappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
