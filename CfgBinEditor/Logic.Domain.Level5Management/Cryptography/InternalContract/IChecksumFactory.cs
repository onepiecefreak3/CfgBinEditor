using CrossCutting.Core.Contract.Aspects;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Cryptography.InternalContract.Exceptions;

namespace Logic.Domain.Level5Management.Cryptography.InternalContract
{
    [MapException(typeof(ChecksumFactoryException))]
    public interface IChecksumFactory
    {
        IChecksum<uint> CreateCrc32();
        IChecksum<uint> CreateCrc32Jam();
        IChecksum<ushort> CreateCrc16();
    }
}
