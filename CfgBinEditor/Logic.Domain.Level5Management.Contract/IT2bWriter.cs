using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Exceptions;

namespace Logic.Domain.Level5Management.Contract
{
    [MapException(typeof(ConfigurationWriterException))]
    public interface IT2bWriter
    {
        Stream Write(T2b config);
    }
}
