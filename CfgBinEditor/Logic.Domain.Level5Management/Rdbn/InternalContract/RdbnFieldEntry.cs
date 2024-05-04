using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Rdbn.InternalContract
{
    internal struct RdbnFieldEntry
    {
        public uint nameHash;
        public short type;
        public short typeCategory;
        public int valueSize;
        public int valueOffset;
        public int valueCount;
    }
}
