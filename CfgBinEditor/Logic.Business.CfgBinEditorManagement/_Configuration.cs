using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.CfgBinEditorManagement
{
    public class CfgBinValueSettingsManagementConfiguration
    {
        [ConfigMap("CfgBinEditor", "ValueSettingsPath")]
        public virtual string ValueSettingsPath { get; set; }
        [ConfigMap("CfgBinEditor", "EntryNamesPath")]
        public virtual string EntryNamesPath { get; set; }
    }
}