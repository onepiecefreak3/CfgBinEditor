using Kryptography.Checksum;
using Kryptography.Checksum.Crc;
using Logic.Domain.Level5Management.Cryptography.InternalContract;

namespace Logic.Domain.Level5Management.Cryptography
{
    internal class ChecksumFactory : IChecksumFactory
    {
        public Checksum<uint> CreateCrc32()
        {
            return Crc32.Crc32B;
        }

        public Checksum<uint> CreateCrc32Jam()
        {
            return Crc32.JamCrc;
        }

        public Checksum<ushort> CreateCrc16()
        {
            return Crc16.X25;
        }
    }
}
