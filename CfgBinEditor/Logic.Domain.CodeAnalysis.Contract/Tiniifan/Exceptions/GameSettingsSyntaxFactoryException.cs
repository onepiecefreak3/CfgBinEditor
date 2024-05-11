using System.Runtime.Serialization;

namespace Logic.Domain.CodeAnalysis.Contract.Tiniifan.Exceptions
{
    [Serializable]
    public class GameSettingsSyntaxFactoryException : Exception
    {
        public GameSettingsSyntaxFactoryException()
        {
        }

        public GameSettingsSyntaxFactoryException(string message) : base(message)
        {
        }

        public GameSettingsSyntaxFactoryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GameSettingsSyntaxFactoryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
