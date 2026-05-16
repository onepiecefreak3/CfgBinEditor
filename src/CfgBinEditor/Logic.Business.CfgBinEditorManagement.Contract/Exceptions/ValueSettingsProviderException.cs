using System.Runtime.Serialization;

namespace Logic.Business.CfgBinEditorManagement.Contract.Exceptions
{
    [Serializable]
    public class ValueSettingsProviderException : Exception
    {
        public ValueSettingsProviderException()
        {
        }

        public ValueSettingsProviderException(string message) : base(message)
        {
        }

        public ValueSettingsProviderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueSettingsProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
