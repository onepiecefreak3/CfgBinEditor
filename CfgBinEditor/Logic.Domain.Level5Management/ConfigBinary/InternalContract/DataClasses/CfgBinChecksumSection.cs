using Logic.Domain.Level5.Contract.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinChecksumSection
    {
        public CfgBinChecksumEntry[] Entries { get; set; }
        public long StringOffset { get; set; }
        public HashType HashType { get; set; }
    }
}
