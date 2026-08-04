using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;

namespace Logic.Business.CfgBinEditorManagement.InternalContract;

public interface IValueSettingsWriter
{
    void Write(IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> settings);
}