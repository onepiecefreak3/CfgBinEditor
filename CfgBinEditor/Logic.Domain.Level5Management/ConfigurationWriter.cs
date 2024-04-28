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
        private readonly IChecksumFactory _checksumFactory;

        public ConfigurationWriter(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksumFactory = checksumFactory;
        }

        public Stream Write(Configuration config)
        {
            Stream stream = new MemoryStream();
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(stream, true);

            Encoding encoding = GetEncoding(config.Encoding);
            IChecksum<uint> checksum = GetChecksum(config.HashType);

            bw.BaseStream.Position = 0x10;
            CfgBinEntryHeader entryHeader = WriteEntries(bw, config.Entries, encoding, checksum, config.ValueLength);

            bw.BaseStream.Position = 0;
            WriteHeader(bw, entryHeader);

            long checksumPartitionOffset = (entryHeader.stringDataOffset + entryHeader.stringDataLength + 0xF) & ~0xF;

            bw.BaseStream.Position = checksumPartitionOffset + 0x10;
            CfgBinChecksumHeader checksumHeader = WriteChecksumEntries(bw, config.Entries, encoding, checksum);

            bw.BaseStream.Position = checksumPartitionOffset;
            WriteChecksumHeader(bw, checksumHeader);

            bw.BaseStream.Position = checksumPartitionOffset + checksumHeader.size;
            WriteFooter(bw, GetCfgBinEncoding(config.Encoding));

            stream.Position = 0;
            return stream;
        }

        private CfgBinEntryHeader WriteEntries(IBinaryWriterX bw, ConfigurationEntry[] configEntries, Encoding encoding, IChecksum<uint> checksum, ValueLength valueLength)
        {
            var header = new CfgBinEntryHeader
            {
                entryCount = (uint)configEntries.Length
            };

            var entryLength = 0;
            foreach (ConfigurationEntry configEntry in configEntries)
                entryLength += 4 + (((int)Math.Ceiling(configEntry.Values.Length / 4f) + 4) & ~3) + configEntry.Values.Length * (int)valueLength;

            header.stringDataOffset = (uint)((0x10 + entryLength + 0xF) & ~0xF);

            uint stringOffset = (uint)bw.BaseStream.Position + header.stringDataOffset - 0x10;
            uint stringOffsetBase = stringOffset;
            var writtenStrings = new Dictionary<string, long>();
            var stringCount = 0u;
            foreach (ConfigurationEntry configEntry in configEntries)
                WriteEntry(bw, configEntry, encoding, checksum, valueLength, stringOffsetBase, writtenStrings, ref stringOffset, ref stringCount);

            bw.WriteAlignment(0x10, 0xFF);

            header.stringDataLength = stringOffset - header.stringDataOffset;
            header.stringDataCount = stringCount;

            bw.BaseStream.Position = stringOffset;
            bw.WriteAlignment(0x10, 0xFF);

            return header;
        }

        private void WriteEntry(IBinaryWriterX bw, ConfigurationEntry configEntry, Encoding encoding, IChecksum<uint> checksum, ValueLength valueLength, uint stringOffsetBase, 
            IDictionary<string, long> writtenStrings, ref uint stringOffset, ref uint stringCount)
        {
            bw.Write(checksum.ComputeValue(configEntry.Name, encoding));
            bw.Write((byte)configEntry.Values.Length);

            var typesWritten = 0;
            byte typeBuffer = 0;
            for (var i = 0; i < configEntry.Values.Length; i++)
            {
                if (typesWritten >= 4)
                {
                    bw.Write(typeBuffer);

                    typeBuffer = 0;
                    typesWritten = 0;
                }

                typeBuffer |= (byte)((int)configEntry.Values[i].Type << (i % 4 * 2));
                typesWritten++;
            }

            if (typesWritten > 0)
                bw.Write(typeBuffer);

            bw.WriteAlignment(4, 0xFF);

            foreach (ConfigurationEntryValue value in configEntry.Values)
            {
                switch (value.Type)
                {
                    case ValueType.String:
                        WriteString(bw, (string)value.Value, encoding, valueLength, stringOffsetBase, writtenStrings, ref stringOffset, ref stringCount);
                        break;

                    case ValueType.Long:
                        WriteValue(bw, (long)value.Value, valueLength);
                        break;

                    case ValueType.Double:
                        switch (valueLength)
                        {
                            case ValueLength.Int:
                                bw.Write((float)(double)value.Value);
                                break;

                            case ValueLength.Long:
                                bw.Write((double)value.Value);
                                break;

                            default:
                                throw new InvalidOperationException($"Unknown value length {valueLength}.");
                        }
                        break;
                }
            }
        }

        private void WriteHeader(IBinaryWriterX bw, CfgBinEntryHeader entryHeader)
        {
            bw.Write(entryHeader.entryCount);
            bw.Write(entryHeader.stringDataOffset);
            bw.Write(entryHeader.stringDataLength);
            bw.Write(entryHeader.stringDataCount);
        }

        private CfgBinChecksumHeader WriteChecksumEntries(IBinaryWriterX bw, ConfigurationEntry[] configEntries, Encoding encoding, IChecksum<uint> checksum)
        {
            string[] names = configEntries.Select(e => e.Name).Distinct().ToArray();

            var header = new CfgBinChecksumHeader
            {
                stringOffset = (uint)((0x10 + names.Length * 8 + 0xF) & ~0xF)
            };

            uint stringOffset = (uint)bw.BaseStream.Position + header.stringOffset - 0x10;
            uint stringOffsetBase = stringOffset;
            var writtenStrings = new Dictionary<string, long>();
            var stringCount = 0u;
            foreach (string name in names)
            {
                bw.Write(checksum.ComputeValue(name, encoding));
                WriteString(bw, name, encoding, ValueLength.Int, stringOffsetBase, writtenStrings, ref stringOffset, ref stringCount);
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

        private void WriteFooter(IBinaryWriterX bw, CfgBinStringEncoding encoding)
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

        private CfgBinStringEncoding GetCfgBinEncoding(StringEncoding encoding)
        {
            switch (encoding)
            {
                case StringEncoding.Sjis:
                    return CfgBinStringEncoding.Sjis;

                case StringEncoding.Utf8:
                    return CfgBinStringEncoding.Utf8;

                // HINT: We don't convert into CfgBinStringEncoding.Utf8_2, as it should support the same character set as UTF8

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }

        private void WriteString(IBinaryWriterX bw, string value, Encoding encoding, ValueLength valueLength, uint stringOffsetBase, IDictionary<string, long> writtenNames,
            ref uint stringOffset, ref uint stringCount)
        {
            if (writtenNames.TryGetValue(value, out long nameOffset))
            {
                WriteValue(bw, nameOffset - stringOffsetBase, valueLength);
                return;
            }

            stringCount++;

            WriteValue(bw, stringOffset - stringOffsetBase, valueLength);
            long entryOffset = bw.BaseStream.Position;

            bw.BaseStream.Position = stringOffset;
            CacheStrings(bw, value, encoding, writtenNames);
            bw.WriteString(value, encoding, false);
            stringOffset = (uint)bw.BaseStream.Position;

            bw.BaseStream.Position = entryOffset;
        }

        private void WriteValue(IBinaryWriterX bw, long value, ValueLength valueLength)
        {
            switch (valueLength)
            {
                case ValueLength.Int:
                    bw.Write((int)value);
                    break;

                case ValueLength.Long:
                    bw.Write(value);
                    break;
            }
        }

        private void CacheStrings(IBinaryWriterX stringWriter, string value, Encoding encoding, IDictionary<string, long> writtenNames)
        {
            long nameOffset = stringWriter.BaseStream.Position;

            do
            {
                if (!writtenNames.ContainsKey(value))
                    writtenNames[value] = nameOffset;

                nameOffset += encoding.GetByteCount(value[..1]);
                value = value.Length > 1 ? value[1..] : string.Empty;
            } while (value.Length > 0);

            if (!writtenNames.ContainsKey(value))
                writtenNames[value] = nameOffset;
        }

        private IChecksum<uint> GetChecksum(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.Crc32Standard:
                    return _checksumFactory.CreateCrc32();

                case HashType.Crc32Jam:
                    return _checksumFactory.CreateCrc32Jam();

                default:
                    throw new InvalidOperationException($"Unknown hash type '{hashType}'.");
            }
        }
    }
}
