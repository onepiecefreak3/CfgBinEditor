using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
