using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses;
using Logic.Domain.Level5.Contract;
using Logic.Domain.Level5.Contract.DataClasses;
using Logic.Domain.Level5.Cryptography.InternalContract;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5
{
    internal class ConfigurationWriter : IConfigurationWriter
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IChecksum<uint> _checksum;

        public ConfigurationWriter(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksum = checksumFactory.CreateCrc32();
        }

        public Stream Write(Configuration config)
        {
            Stream stream = new MemoryStream();
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(stream, true);

            Encoding encoding = GetEncoding(config.Encoding);

            bw.BaseStream.Position = 0x10;
            CfgBinHeader header = WriteEntries(bw, config.Entries, encoding);

            bw.BaseStream.Position = 0;
            WriteHeader(bw, header);

            long checksumPartitionOffset = (header.stringDataOffset + header.stringDataLength + 0xF) & ~0xF;

            bw.BaseStream.Position = checksumPartitionOffset + 0x10;
            CfgBinChecksumHeader checksumHeader = WriteChecksumEntries(bw, config.Entries, encoding);

            bw.BaseStream.Position = checksumPartitionOffset;
            WriteChecksumHeader(bw, checksumHeader);

            bw.BaseStream.Position = checksumPartitionOffset + checksumHeader.size;
            WriteFooter(bw, config.Encoding);

            stream.Position = 0;
            return stream;
        }

        private CfgBinHeader WriteEntries(IBinaryWriterX bw, ConfigurationEntry[] configEntries, Encoding encoding)
        {
            var header = new CfgBinHeader
            {
                entryCount = (uint)configEntries.Length
            };

            var entryLength = 0;
            foreach (ConfigurationEntry configEntry in configEntries)
                entryLength += 4 + (((int)Math.Ceiling(configEntry.Values.Length / 4f) + 4) & ~3) + configEntry.Values.Length * 4;

            header.stringDataOffset = (uint)((0x10 + entryLength + 0xF) & ~0xF);

            uint stringOffset = (uint)bw.BaseStream.Position + header.stringDataOffset - 0x10;
            uint stringOffsetBase = stringOffset;
            var stringCount = 0u;
            foreach (ConfigurationEntry configEntry in configEntries)
                WriteEntry(bw, configEntry, encoding, stringOffsetBase, ref stringOffset, ref stringCount);

            bw.WriteAlignment(0x10, 0xFF);

            header.stringDataLength = stringOffset - header.stringDataOffset;
            header.stringDataCount = stringCount;

            bw.BaseStream.Position = stringOffset;
            bw.WriteAlignment(0x10, 0xFF);

            return header;
        }

        private void WriteEntry(IBinaryWriterX bw, ConfigurationEntry configEntry, Encoding encoding, uint stringOffsetBase, ref uint stringOffset, ref uint stringCount)
        {
            bw.Write(_checksum.ComputeValue(configEntry.Name, encoding));
            bw.Write((byte)configEntry.Values.Length);

            byte typeBuffer = 0;
            for (var i = 0; i < configEntry.Values.Length; i++)
            {
                if (i != 0 && i % 4 == 0)
                {
                    bw.Write(typeBuffer);
                    typeBuffer = 0;
                }

                typeBuffer |= (byte)((int)configEntry.Values[i].Type << (i % 4 * 2));
            }

            if (configEntry.Values.Length != 0 && configEntry.Values.Length == 1 || (configEntry.Values.Length - 1) % 4 != 0)
                bw.Write(typeBuffer);

            bw.WriteAlignment(4, 0xFF);

            foreach (ConfigurationEntryValue value in configEntry.Values)
            {
                switch (value.Type)
                {
                    case ValueType.String:
                        WriteString(bw, (string)value.Value, encoding, stringOffsetBase, ref stringOffset, ref stringCount);
                        break;

                    case ValueType.UInt:
                        bw.Write((uint)value.Value);
                        break;

                    case ValueType.Float:
                        bw.Write((float)value.Value);
                        break;
                }
            }
        }

        private void WriteString(IBinaryWriterX bw, string value, Encoding encoding, uint stringOffsetBase, ref uint stringOffset, ref uint stringCount)
        {
            stringCount++;

            bw.Write(stringOffset - stringOffsetBase);
            long entryOffset = bw.BaseStream.Position;

            bw.BaseStream.Position = stringOffset;
            bw.WriteString(value, encoding, false);
            stringOffset = (uint)bw.BaseStream.Position;

            bw.BaseStream.Position = entryOffset;
        }

        private void WriteHeader(IBinaryWriterX bw, CfgBinHeader header)
        {
            bw.Write(header.entryCount);
            bw.Write(header.stringDataOffset);
            bw.Write(header.stringDataLength);
            bw.Write(header.stringDataCount);
        }

        private CfgBinChecksumHeader WriteChecksumEntries(IBinaryWriterX bw, ConfigurationEntry[] configEntries, Encoding encoding)
        {
            string[] names = configEntries.Select(e => e.Name).Distinct().ToArray();

            var header = new CfgBinChecksumHeader
            {
                stringOffset = (uint)((0x10 + names.Length * 8 + 0xF) & ~0xF)
            };

            uint stringOffset = (uint)bw.BaseStream.Position + header.stringOffset - 0x10;
            uint stringOffsetBase = stringOffset;
            var stringCount = 0u;
            foreach (string name in names)
            {
                uint checksum = _checksum.ComputeValue(name, encoding);

                bw.Write(checksum);
                WriteString(bw, name, encoding, stringOffsetBase, ref stringOffset, ref stringCount);
            }

            bw.WriteAlignment(0x10, 0xFF);

            header.count = stringCount;
            header.size = (uint)(header.stringOffset + ((stringOffset - stringOffsetBase + 0xF) & ~0xF));
            header.stringSize = stringOffset - stringOffsetBase;

            bw.BaseStream.Position = stringOffset;
            bw.WriteAlignment(0x10, 0xFF);

            return header;
        }

        private void WriteChecksumHeader(IBinaryWriterX bw, CfgBinChecksumHeader header)
        {
            bw.Write(header.size);
            bw.Write(header.count);
            bw.Write(header.stringOffset);
            bw.Write(header.stringSize);
        }

        private void WriteFooter(IBinaryWriterX bw, StringEncoding encoding)
        {
            bw.WriteString("\x1t2b", Encoding.ASCII, false, false);
            bw.Write((short)0x1fe);
            bw.Write((short)encoding);
            bw.Write((short)1);

            bw.WriteAlignment(0x10, 0xFF);
        }

        private Encoding GetEncoding(StringEncoding encoding)
        {
            switch (encoding)
            {
                case StringEncoding.Sjis:
                    return Encoding.GetEncoding("Shift-JIS");

                case StringEncoding.Utf8:
                    return Encoding.UTF8;

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }
    }
}
