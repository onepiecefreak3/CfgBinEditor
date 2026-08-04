using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Business.CfgBinEditorManagement.Contract;

public interface IEntryNamesProvider
{
    bool TryGetError(out Exception? error);
    bool TryGetName(string game, T2bEntry entry, ValueLength length, out string? name);
}