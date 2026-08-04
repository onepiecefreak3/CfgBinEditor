using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Contract;

public interface IT2bReader
{
    T2b? Read(Stream input);
}