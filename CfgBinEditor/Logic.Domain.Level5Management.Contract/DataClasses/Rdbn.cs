using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.Contract.DataClasses
{
    public class Rdbn
    {
        public RdbnTypeDeclaration[] Types { get; set; }
        public RdbnListEntry[] Lists { get; set; }
    }
}
