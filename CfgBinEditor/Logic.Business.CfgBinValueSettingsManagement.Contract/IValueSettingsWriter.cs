using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Business.CfgBinValueSettingsManagement.Contract.Exceptions;

namespace Logic.Business.CfgBinValueSettingsManagement.Contract
{
    [MapException(typeof(ValueSettingsWriterException))]
    public interface IValueSettingsWriter
    {
        void Write(IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> settings);
    }
}
