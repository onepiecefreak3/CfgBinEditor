using Logic.Domain.Level5Management.Contract.DataClasses;

namespace Logic.Domain.Level5Management.Contract
{
    public interface IRdbnReader
    {
        Rdbn? Read(Stream input);
    }
}
