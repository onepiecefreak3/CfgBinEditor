using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
