using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5.Contract.DataClasses;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinEntrySection
    {
        public CfgBinEntry[] Entries { get; set; }
        public long StringOffset { get; set; }
        public ValueLength ValueLength { get; set; }
    }
}
