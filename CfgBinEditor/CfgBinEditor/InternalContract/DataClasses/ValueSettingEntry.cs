using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CfgBinEditor.InternalContract.DataClasses
{
    public class ValueSettingEntry
    {
        public static ValueSettingEntry Empty => new() { Name = string.Empty, IsHex = false };

        public string Name { get; set; }
        public bool IsHex { get; set; }
    }
}
