using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions
{
    public class GameSettingsParserException : Exception
    {
        public GameSettingsParserException()
        {
        }

        public GameSettingsParserException(string message) : base(message)
        {
        }

        public GameSettingsParserException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GameSettingsParserException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
