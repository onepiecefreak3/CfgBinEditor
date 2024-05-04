using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.ConfigBinary.InternalContract.DataClasses
{
    internal struct T2bChecksumSection
    {
        public T2bChecksumEntry[] Entries { get; set; }
        public long StringOffset { get; set; }
        public HashType HashType { get; set; }
    }
}
