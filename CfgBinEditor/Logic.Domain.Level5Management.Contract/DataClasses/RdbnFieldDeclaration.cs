using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Contract.DataClasses
{
    [DebuggerDisplay("Field {Name}")]
    public class RdbnFieldDeclaration
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Size { get; set; }
        public FieldType FieldType { get; set; }
        public FieldTypeCategory FieldTypeCategory { get; set; }
    }
}
