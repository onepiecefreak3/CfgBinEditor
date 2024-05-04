using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Contract.Exceptions;

namespace Logic.Domain.Level5Management.Contract
{
    [MapException(typeof(ConfigurationReaderException))]
    public interface IT2bReader
    {
        T2b? Read(Stream input);
    }
}
