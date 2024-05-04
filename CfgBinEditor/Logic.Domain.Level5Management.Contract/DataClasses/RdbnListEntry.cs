using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Contract.DataClasses
{
    [DebuggerDisplay("Root {Name}")]
    public class RdbnListEntry
    {
        public string Name { get; set; }
        public int TypeIndex { get; set; }
        public object[][][] Values { get; set; }
    }
}
