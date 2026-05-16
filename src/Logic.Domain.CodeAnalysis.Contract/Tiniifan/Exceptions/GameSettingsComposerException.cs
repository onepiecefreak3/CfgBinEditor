using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions
{
    public class GameSettingsComposerException : Exception
    {
        public GameSettingsComposerException()
        {
        }

        public GameSettingsComposerException(string message) : base(message)
        {
        }

        public GameSettingsComposerException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GameSettingsComposerException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
