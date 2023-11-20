using CrossCutting.Core.Contract.Aspects;
using CrossCutting.Core.Contract.Configuration.Exceptions;
using System;

namespace CrossCutting.Core.Contract.Configuration
{
    [MapException(typeof(ConfigurationException))]
    public interface IConfigObjectProvider
    {
        TConfig Get<TConfig>();
        object Get(Type configType);
    }
}