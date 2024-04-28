using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses
{
    internal enum CfgBinStringEncoding : short
    {
        Sjis,
        Utf8,
        Utf8_2 = 256,
        Utf8_3 = 257
    }
}
