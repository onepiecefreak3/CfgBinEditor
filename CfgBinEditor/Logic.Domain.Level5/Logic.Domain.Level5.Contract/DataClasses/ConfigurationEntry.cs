using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.Contract.DataClasses
{
    public class ConfigurationEntry
    {
        public string Name { get; set; }
        public ConfigurationEntryValue[] Values { get; set; }
    }
}
