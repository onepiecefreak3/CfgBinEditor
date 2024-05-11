using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Cryptography.InternalContract;
using Logic.Domain.Level5Management.T2b.InternalContract.DataClasses;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5Management
{
    internal class T2bWriter : IT2bWriter
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IChecksumFactory _checksumFactory;

        public T2bWriter(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksumFactory = checksumFactory;
        }

        public Stream Write(Contract.DataClasses.T2b config)
        {
            Stream stream = new MemoryStream();
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(stream, true);

            Encoding encoding = GetEncoding(config.Encoding);
            IChecksum<uint> checksum = GetChecksum(config.HashType);

            bw.BaseStream.Position = 0x10;
            T2bEntryHeader entryHeader = WriteEntries(bw, config.Entries, encoding, checksum, config.ValueLength);

            bw.BaseStream.Position = 0;
            WriteHeader(bw, entryHeader);

            long checksumPartitionOffset = (entryHeader.stringDataOffset + entryHeader.stringDataLength + 0xF) & ~0xF;

            bw.BaseStream.Position = checksumPartitionOffset + 0x10;
            T2bChecksumHeader checksumHeader = WriteChecksumEntries(bw, config.Entries, encoding, checksum);

            bw.BaseStream.Position = checksumPartitionOffset;
            WriteChecksumHeader(bw, checksumHeader);

            bw.BaseStream.Position = checksumPartitionOffset + checksumHeader.size;
            WriteFooter(bw, GetCfgBinEncoding(config.Encoding));

            stream.Position = 0;
            return stream;
        }

        private T2bEntryHeader WriteEntries(IBinaryWriterX bw, Contract.DataClasses.T2bEntry[] configEntries, Encoding encoding, IChecksum<uint> checksum, ValueLength valueLength)
        {
            var header = new T2bEntryHeader
            {
                entryCount = (uint)configEntries.Length
            };

            var entryLength = 0;
            foreach (Contract.DataClasses.T2bEntry configEntry in configEntries)
                entryLength += 4 + (((int)Math.Ceiling(configEntry.Values.Length / 4f) + 4) & ~3) + configEntry.Values.Length * (int)valueLength;

            header.stringDataOffset = (uint)((0x10 + entryLength + 0xF) & ~0xF);

            uint stringOffset = (uint)bw.BaseStream.Position + header.stringDataOffset - 0x10;
            uint stringOffsetBase = stringOffset;
            var writtenStrings = new Dictionary<string, long>();
            var stringCount = 0u;
            foreach (Contract.DataClasses.T2bEntry configEntry in configEntries)
                WriteEntry(bw, configEntry, encoding, checksum, valueLength, stringOffsetBase, writtenStrings, ref stringOffset, ref stringCount);

            bw.WriteAlignment(0x10, 0xFF);

            header.stringDataLength = stringOffset - header.stringDataOffset;
            header.stringDataCount = stringCount;

            bw.BaseStream.Position = stringOffset;
            bw.WriteAlignment(0x10, 0xFF);

            return header;
        }

        private void WriteEntry(IBinaryWriterX bw, Contract.DataClasses.T2bEntry configEntry, Encoding encoding, IChecksum<uint> checksum, ValueLength valueLength, uint stringOffsetBase, 
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

            foreach (T2bEntryValue value in configEntry.Values)
            {
                switch (value.Type)
                {
                    case ValueType.String:
                        WriteString(bw, (string)value.Value, encoding, valueLength, stringOffsetBase, writtenStrings, ref stringOffset, ref stringCount);
                        break;

                    case ValueType.Integer:
                        switch (valueLength)
                        {
                            case ValueLength.Int:
                                bw.Write((int)value.Value);
                                break;

                            case ValueLength.Long:
                                bw.Write((long)value.Value);
                                break;

                            default:
                                throw new InvalidOperationException($"Unknown value length {valueLength}.");
                        }
                        break;

                    case ValueType.FloatingPoint:
                        switch (valueLength)
                        {
                            case ValueLength.Int:
                                bw.Write((float)value.Value);
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

        private void WriteHeader(IBinaryWriterX bw, T2bEntryHeader entryHeader)
        {
            bw.Write(entryHeader.entryCount);
            bw.Write(entryHeader.stringDataOffset);
            bw.Write(entryHeader.stringDataLength);
            bw.Write(entryHeader.stringDataCount);
        }

        private T2bChecksumHeader WriteChecksumEntries(IBinaryWriterX bw, Contract.DataClasses.T2bEntry[] configEntries, Encoding encoding, IChecksum<uint> checksum)
        {
            string[] names = configEntries.Select(e => e.Name).Distinct().ToArray();

            var header = new T2bChecksumHeader
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

        private void WriteChecksumHeader(IBinaryWriterX bw, T2bChecksumHeader header)
        {
            bw.Write(header.size);
            bw.Write(header.count);
            bw.Write(header.stringOffset);
            bw.Write(header.stringSize);
        }

        private void WriteFooter(IBinaryWriterX bw, T2bStringEncoding encoding)
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

        private T2bStringEncoding GetCfgBinEncoding(StringEncoding encoding)
        {
            switch (encoding)
            {
                case StringEncoding.Sjis:
                    return T2bStringEncoding.Sjis;

                case StringEncoding.Utf8:
                    return T2bStringEncoding.Utf8;

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
            CacheStrings(stringOffset, value, encoding, writtenNames);
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

                default:
                    throw new InvalidOperationException($"Unknown value length {valueLength}.");
            }
        }

        private void CacheStrings(long position, string value, Encoding encoding, IDictionary<string, long> writtenNames)
        {
            do
            {
                if (!writtenNames.ContainsKey(value))
                    writtenNames[value] = position;

                position += encoding.GetByteCount(value[..1]);
                value = value.Length > 1 ? value[1..] : string.Empty;
            } while (value.Length > 0);

            if (!writtenNames.ContainsKey(value))
                writtenNames[value] = position;
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
