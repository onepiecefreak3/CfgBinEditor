using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.Contract.DataClasses
{
    public class Configuration
    {
        public ConfigurationEntry[] Entries { get; set; }
        public StringEncoding Encoding { get; set; }
        public ValueLength ValueLength { get; set; }
        public HashType HashType { get; set; }
    }
}
