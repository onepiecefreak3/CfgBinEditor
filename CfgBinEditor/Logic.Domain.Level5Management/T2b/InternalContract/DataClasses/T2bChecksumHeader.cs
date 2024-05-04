using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.ConfigBinary.InternalContract.DataClasses
{
    internal struct T2bChecksumHeader
    {
        public uint size;
        public uint count;
        public uint stringOffset;
        public uint stringSize;
    }
}
