using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinEditorManagement.Contract.Exceptions;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Business.CfgBinEditorManagement.Contract
{
    [MapException(typeof(EntryNameProviderException))]
    public interface IEntryNamesProvider
    {
        bool TryGetError(out Exception? error);
        bool TryGetName(string game, T2bEntry entry, ValueLength length, out string? name);
    }
}
