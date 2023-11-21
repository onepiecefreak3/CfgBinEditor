using System.Runtime.Serialization;

namespace Logic.Business.CfgBinValueSettingsManagement.Contract.Exceptions
{
    [Serializable]
    public class ValueSettingsReaderException : Exception
    {
        public ValueSettingsReaderException()
        {
        }

        public ValueSettingsReaderException(string message) : base(message)
        {
        }

        public ValueSettingsReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueSettingsReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
