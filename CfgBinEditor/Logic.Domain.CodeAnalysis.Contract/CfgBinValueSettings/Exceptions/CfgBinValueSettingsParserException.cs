using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions
{
    public class CfgBinValueSettingsParserException : Exception
    {
        public CfgBinValueSettingsParserException()
        {
        }

        public CfgBinValueSettingsParserException(string message) : base(message)
        {
        }

        public CfgBinValueSettingsParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CfgBinValueSettingsParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
