using System;
using System.Runtime.Serialization;

namespace CfgBinEditor.InternalContract.Exceptions
{
    [Serializable]
    public class ComponentFactoryException : Exception
    {
        public ComponentFactoryException()
        {
        }

        public ComponentFactoryException(string message) : base(message)
        {
        }

        public ComponentFactoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ComponentFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
