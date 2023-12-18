using System.Runtime.Serialization;

namespace Logic.Business.CfgBinValueSettingsManagement.Contract.Exceptions
{
    [Serializable]
    public class ValueSettingsWriterException : Exception
    {
        public ValueSettingsWriterException()
        {
        }

        public ValueSettingsWriterException(string message) : base(message)
        {
        }

        public ValueSettingsWriterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueSettingsWriterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
