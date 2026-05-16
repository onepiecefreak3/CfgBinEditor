using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace CfgBinEditor
{
    public class CfgBinEditorConfiguration
    {
        [ConfigMap("UI.CfgBinEditor.Resources", "LocalizationPath")]
        public virtual string LocalizationPath { get; set; } = "resources/langs";

        [ConfigMap("UI.CfgBinEditor.Resources", "DefaultLocale")]
        public virtual string DefaultLocale { get; set; }
    }
}
