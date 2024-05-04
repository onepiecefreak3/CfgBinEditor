using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Contract
{
    public interface IRdbnWriter
    {
        Stream Write(Rdbn config);
    }
}
