using Logic.Domain.Level5Management.Contract.DataClasses;

namespace CfgBinEditor.InternalContract.DataClasses
{
    public class T2bFile
    {
        public required string FilePath { get; init; }
        public required T2b Data { get; init; }
    }
}
