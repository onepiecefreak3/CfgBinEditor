using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.CodeAnalysis.Contract.CfgBinValueSettings.Exceptions
{
    [Serializable]
    public class CfgBinValueSettingsSyntaxFactoryException : Exception
    {
        public CfgBinValueSettingsSyntaxFactoryException()
        {
        }

        public CfgBinValueSettingsSyntaxFactoryException(string message) : base(message)
        {
        }

        public CfgBinValueSettingsSyntaxFactoryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CfgBinValueSettingsSyntaxFactoryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
