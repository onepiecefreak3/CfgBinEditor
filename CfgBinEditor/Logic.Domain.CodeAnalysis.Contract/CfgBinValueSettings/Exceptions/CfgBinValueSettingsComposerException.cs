using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions
{
    public class CfgBinValueSettingsComposerException : Exception
    {
        public CfgBinValueSettingsComposerException()
        {
        }

        public CfgBinValueSettingsComposerException(string message) : base(message)
        {
        }

        public CfgBinValueSettingsComposerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CfgBinValueSettingsComposerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
