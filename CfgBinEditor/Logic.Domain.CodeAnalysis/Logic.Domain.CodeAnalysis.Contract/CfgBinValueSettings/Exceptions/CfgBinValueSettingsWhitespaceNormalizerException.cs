using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions
{
    public class CfgBinValueSettingsWhitespaceNormalizerException : Exception
    {
        public CfgBinValueSettingsWhitespaceNormalizerException()
        {
        }

        public CfgBinValueSettingsWhitespaceNormalizerException(string message) : base(message)
        {
        }

        public CfgBinValueSettingsWhitespaceNormalizerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CfgBinValueSettingsWhitespaceNormalizerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
