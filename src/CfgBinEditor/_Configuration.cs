using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace CfgBinEditor;

[ConfigurationCategory("UI.CfgBinEditor.Resources")]
public class CfgBinEditorConfiguration
{
    public string LocalizationPath { get; set; } = "resources/langs";

    public string DefaultLocale { get; set; } = "en";
}