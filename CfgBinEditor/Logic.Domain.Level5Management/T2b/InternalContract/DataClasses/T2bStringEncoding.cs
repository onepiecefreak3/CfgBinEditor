using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Level5Management.ConfigBinary.InternalContract.DataClasses
{
    internal enum T2bStringEncoding : short
    {
        Sjis,
        Utf8,
        Utf8_2 = 256,
        Utf8_3 = 257
    }
}
