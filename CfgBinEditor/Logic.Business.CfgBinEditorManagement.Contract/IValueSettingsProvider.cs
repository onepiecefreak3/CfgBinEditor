using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Business.CfgBinEditorManagement.Contract.Exceptions;

namespace Logic.Business.CfgBinEditorManagement.Contract
{
    [MapException(typeof(ValueSettingsProviderException))]
    public interface IValueSettingsProvider
    {
        bool TryGetError(out Exception? error);

        string[] GetGames();
        void AddGame(string game);

        ValueSettingEntry GetEntrySettings(string game, string entryName, int index);
        void SetEntrySettings(string game, string entryName, int index, ValueSettingEntry entry);

        void Persist();
    }
}
