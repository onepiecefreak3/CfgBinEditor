using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions
{
    public class GameSettingsWhitespaceNormalizerException : Exception
    {
        public GameSettingsWhitespaceNormalizerException()
        {
        }

        public GameSettingsWhitespaceNormalizerException(string message) : base(message)
        {
        }

        public GameSettingsWhitespaceNormalizerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GameSettingsWhitespaceNormalizerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
