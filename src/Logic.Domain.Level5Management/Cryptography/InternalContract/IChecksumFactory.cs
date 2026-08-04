using Kryptography.Checksum;

namespace Logic.Domain.Level5Management.Cryptography.InternalContract;

public interface IChecksumFactory
{
    Checksum<uint> CreateCrc32();
    Checksum<uint> CreateCrc32Jam();
    Checksum<ushort> CreateCrc16();
}