using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal struct CfgBinFooter
    {
        public uint magic;
        public short unk1;
        public short encoding;
        public short unk2;
    }
}
