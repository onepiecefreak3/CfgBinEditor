using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Configuration.DataClasses;

namespace CfgBinEditor
{
    public class CfgBinEditorConfiguration
    {
        [ConfigMap("CfgBinEditor", "ValueSettingsPath")]
        public virtual string ValueSettingsPath { get; set; } = "MyTags.txt";
    }
}
