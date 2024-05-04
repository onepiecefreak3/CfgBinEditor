using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Contract
{
    public interface IRdbnReader
    {
        Rdbn? Read(Stream input);
    }
}
