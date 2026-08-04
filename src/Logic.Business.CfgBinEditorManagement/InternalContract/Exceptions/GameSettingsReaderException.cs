using System.Runtime.Serialization;

namespace Logic.Business.CfgBinEditorManagement.InternalContract.Exceptions;

[Serializable]
public class GameSettingsReaderException : Exception
{
    public GameSettingsReaderException()
    {
    }

    public GameSettingsReaderException(string message) : base(message)
    {
    }

    public GameSettingsReaderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}