using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5.Contract.DataClasses;
using Logic.Domain.Level5.Contract.Exceptions;

namespace Logic.Domain.Level5.Contract
{
    [MapException(typeof(ConfigurationReaderException))]
    public interface IConfigurationReader
    {
        Configuration? Read(Stream input);
    }
}
