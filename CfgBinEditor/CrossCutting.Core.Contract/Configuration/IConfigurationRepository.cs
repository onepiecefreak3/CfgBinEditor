using CrossCutting.Core.Contract.Aspects;
using CrossCutting.Core.Contract.Configuration.DataClasses;
using CrossCutting.Core.Contract.Configuration.Exceptions;
using System.Collections.Generic;

namespace CrossCutting.Core.Contract.Configuration
{
    [MapException(typeof(ConfigurationException))]
    public interface IConfigurationRepository
    {
        IEnumerable<ConfigCategory> Load();
    }
}