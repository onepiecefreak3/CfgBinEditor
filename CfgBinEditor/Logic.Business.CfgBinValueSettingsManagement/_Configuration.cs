using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.CfgBinValueSettingsManagement
{
    public class CfgBinValueSettingsManagementConfiguration
    {
        [ConfigMap("CfgBinEditor", "ValueSettingsPath")]
        public virtual string ValueSettingsPath { get; set; } = "MyTags.txt";
    }
}