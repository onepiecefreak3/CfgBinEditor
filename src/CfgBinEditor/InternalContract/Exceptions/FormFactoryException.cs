using System;
using System.Runtime.Serialization;

namespace CfgBinEditor.InternalContract.Exceptions
{
    [Serializable]
    internal class FormFactoryException : Exception
    {
        public FormFactoryException()
        {
        }

        public FormFactoryException(string message) : base(message)
        {
        }

        public FormFactoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FormFactoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
