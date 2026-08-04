using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace Logic.Business.CfgBinEditorManagement;

[ConfigurationCategory("CfgBinEditor")]
public class CfgBinValueSettingsManagementConfiguration
{
    public string? ValueSettingsPath { get; set; }

    public string? EntryNamesPath { get; set; }
}