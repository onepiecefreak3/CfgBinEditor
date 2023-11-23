using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5.Contract.DataClasses;
using Logic.Domain.Level5.Contract.Exceptions;

namespace Logic.Domain.Level5.Contract
{
    [MapException(typeof(ConfigurationWriterException))]
    public interface IConfigurationWriter
    {
        Stream Write(Configuration config);
    }
}
