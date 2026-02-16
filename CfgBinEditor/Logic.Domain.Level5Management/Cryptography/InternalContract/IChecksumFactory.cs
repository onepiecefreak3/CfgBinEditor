using CrossCutting.Core.Contract.Aspects;
using Kryptography.Checksum;
using Logic.Domain.Level5Management.Cryptography.InternalContract.Exceptions;

namespace Logic.Domain.Level5Management.Cryptography.InternalContract
{
    [MapException(typeof(ChecksumFactoryException))]
    public interface IChecksumFactory
    {
        Checksum<uint> CreateCrc32();
        Checksum<uint> CreateCrc32Jam();
        Checksum<ushort> CreateCrc16();
    }
}
