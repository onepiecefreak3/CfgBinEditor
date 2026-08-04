using System.Runtime.Serialization;

namespace Logic.Business.CfgBinEditorManagement.Contract.Exceptions;

[Serializable]
public class EntryNameProviderException : Exception
{
    public EntryNameProviderException()
    {
    }

    public EntryNameProviderException(string message) : base(message)
    {
    }

    public EntryNameProviderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}