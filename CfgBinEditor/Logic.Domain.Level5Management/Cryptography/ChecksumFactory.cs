﻿using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract.DataClasses;
using Logic.Domain.Level5Management.Cryptography.InternalContract;

namespace Logic.Domain.Level5Management.Cryptography
{
    internal class ChecksumFactory : IChecksumFactory
    {
        private readonly ICrcChecksumFactory _crcFactory;

        public ChecksumFactory(ICrcChecksumFactory crcFactory)
        {
            _crcFactory = crcFactory;
        }

        public IChecksum<uint> CreateCrc32()
        {
            return _crcFactory.CreateCrc32(Crc32Type.Standard);
        }

        public IChecksum<uint> CreateCrc32Jam()
        {
            return _crcFactory.CreateCrc32(Crc32Type.JamCrc);
        }

        public IChecksum<ushort> CreateCrc16()
        {
            return _crcFactory.CreateCrc16(Crc16Type.X25);
        }
    }
}
