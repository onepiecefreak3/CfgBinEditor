using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    internal class FileSaveRequestMessage
    {
        public IDictionary<ConfigurationForm, string> ConfigForms { get; }

        public FileSaveRequestMessage(IDictionary<ConfigurationForm, string> configForms)
        {
            ConfigForms = configForms;
        }
    }
}
