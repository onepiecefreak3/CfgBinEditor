using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.ConfigBinary.InternalContract.DataClasses
{
    internal struct T2bEntrySection
    {
        public T2bEntry[] Entries { get; set; }
        public long StringOffset { get; set; }
        public ValueLength ValueLength { get; set; }
    }
}
