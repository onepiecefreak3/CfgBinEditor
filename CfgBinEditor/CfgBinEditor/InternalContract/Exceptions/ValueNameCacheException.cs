using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.InternalContract.Exceptions
{
    [Serializable]
    public class ValueNameCacheException : Exception
    {
        public ValueNameCacheException()
        {
        }

        public ValueNameCacheException(string message) : base(message)
        {
        }

        public ValueNameCacheException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueNameCacheException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
