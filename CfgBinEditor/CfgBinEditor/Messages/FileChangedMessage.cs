using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CfgBinEditor.Forms;

namespace CfgBinEditor.Messages
{
    public class FileChangedMessage
    {
        public ConfigurationForm ConfigForm { get; }

        public FileChangedMessage(ConfigurationForm configForm)
        {
            ConfigForm = configForm;
        }
    }
}
