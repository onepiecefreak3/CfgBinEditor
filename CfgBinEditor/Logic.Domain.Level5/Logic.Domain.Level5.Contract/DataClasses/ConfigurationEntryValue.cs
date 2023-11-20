using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.Contract.DataClasses
{
    public class ConfigurationEntryValue
    {
        public ValueType Type { get; set; }
        public object Value { get; set; }
    }
}
