using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Contract;

public interface IT2bWriter
{
    Stream Write(T2b config);
}