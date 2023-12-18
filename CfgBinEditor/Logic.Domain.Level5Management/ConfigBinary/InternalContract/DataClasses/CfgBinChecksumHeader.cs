using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinChecksumHeader
    {
        public uint size;
        public uint count;
        public uint stringOffset;
        public uint stringSize;
    }
}
