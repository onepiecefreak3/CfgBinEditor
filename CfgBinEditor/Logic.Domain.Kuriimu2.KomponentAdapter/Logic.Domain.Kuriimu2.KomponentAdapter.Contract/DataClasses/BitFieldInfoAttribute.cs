﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Domain.Kuriimu2.KomponentAdapter.Contract.DataClasses
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BitFieldInfoAttribute : Attribute
    {
        public int BlockSize = 4;
        public BitOrder BitOrder = BitOrder.Default;
    }
}
