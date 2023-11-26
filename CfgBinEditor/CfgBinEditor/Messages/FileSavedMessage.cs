using CfgBinEditor.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.Messages
{
    internal class FileSavedMessage
    {
        public ConfigurationForm ConfigForm { get; }
        public Exception Error { get; }

        public FileSavedMessage(ConfigurationForm configForm, Exception e)
        {
            ConfigForm = configForm;
            Error = e;
        }
    }
}
