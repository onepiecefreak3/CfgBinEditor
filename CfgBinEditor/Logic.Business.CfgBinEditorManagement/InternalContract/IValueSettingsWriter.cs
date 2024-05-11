using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinEditorManagement.Contract.DataClasses;
using Logic.Business.CfgBinEditorManagement.InternalContract.Exceptions;

namespace Logic.Business.CfgBinEditorManagement.InternalContract
{
    [MapException(typeof(ValueSettingsWriterException))]
    public interface IValueSettingsWriter
    {
        void Write(IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> settings);
    }
}
