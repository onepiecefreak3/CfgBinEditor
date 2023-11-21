using CrossCutting.Core.Contract.Aspects;
using Logic.Business.CfgBinValueSettingsManagement.Contract.DataClasses;
using Logic.Business.CfgBinValueSettingsManagement.Contract.Exceptions;

namespace Logic.Business.CfgBinValueSettingsManagement.Contract
{
    [MapException(typeof(ValueSettingsReaderException))]
    public interface IValueSettingsReader
    {
        IDictionary<string, IDictionary<string, IList<ValueSettingEntry>>> Read();
    }
}
