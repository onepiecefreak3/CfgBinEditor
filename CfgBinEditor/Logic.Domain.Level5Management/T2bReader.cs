using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Cryptography.InternalContract;
using Logic.Domain.Level5Management.T2b.InternalContract.DataClasses;
using T2bEntry = Logic.Domain.Level5Management.T2b.InternalContract.DataClasses.T2bEntry;
using ValueType = Logic.Domain.Level5Management.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5Management
{
    internal class T2bReader : IT2bReader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IChecksumFactory _checksumFactory;

        private const int MinimumSize_ = 0x30;

        public T2bReader(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksumFactory = checksumFactory;
        }

        public Contract.DataClasses.T2b? Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input);

            // Read t2b footer
            if (br.BaseStream.Length < MinimumSize_)
                return null;

            br.BaseStream.Position = input.Length - 0x10;

            T2bFooter footer = ReadFooter(br);
            if (footer.magic != 0x62327401)
                return null;

            // Read t2b entry section
            br.BaseStream.Position = 0;

            T2bEntrySection? entrySection = ReadEntrySection(br);
            if (entrySection == null)
                return null;

            // Read value string section
            byte[] valueStringData;
            if (entrySection.Value.StringSize > 0)
            {
                br.BaseStream.Position = entrySection.Value.StringOffset;
                valueStringData = br.ReadBytes(entrySection.Value.StringSize);
            }
            else
            {
                valueStringData = Array.Empty<byte>();
            }

            br.SeekAlignment();

            // Read t2b checksum section
            var encoding = (T2bStringEncoding)footer.encoding;
            long checksumPosition = br.BaseStream.Position;

            T2bChecksumSection? checksumSection = ReadChecksumSection(br);

            // Read checksum string section
            br.BaseStream.Position = checksumPosition + checksumSection.Value.StringOffset;

            byte[] checksumStringData;
            if (checksumSection.Value.StringSize > 0)
            {
                br.BaseStream.Position = checksumSection.Value.StringOffset;
                checksumStringData = br.ReadBytes(checksumSection.Value.StringSize);
            }
            else
            {
                checksumStringData = Array.Empty<byte>();
            }

            HashType hashType = default;
            if (checksumSection.Value.Entries.Length > 0)
            {
                if (!TryDetectHashType(checksumSection.Value.Entries[0], checksumStringData, encoding, out hashType))
                    return null;
            }

            // Create final object
            return CreateConfiguration(entrySection.Value, checksumSection.Value, valueStringData, checksumStringData, encoding, hashType);
        }

        #region Entries

        private T2bEntrySection? ReadEntrySection(IBinaryReaderX br)
        {
            long sectionPosition = br.BaseStream.Position;

            T2bEntryHeader entryHeader = ReadEntryHeader(br);
            long stringOffset = sectionPosition + entryHeader.stringDataOffset;

            ValueLength valueLength = default;
            T2bEntry[] entries = Array.Empty<T2bEntry>();

            if (entryHeader.entryCount > 0)
            {
                if (!TryDetectValueLength(br, entryHeader.entryCount, stringOffset, out valueLength))
                    return null;

                entries = ReadEntries(br, entryHeader.entryCount, valueLength);
            }

            return new T2bEntrySection
            {
                Entries = entries,
                StringOffset = stringOffset,
                StringSize = (int)entryHeader.stringDataLength,
                ValueLength = valueLength
            };
        }

        private T2bEntryHeader ReadEntryHeader(IBinaryReaderX br)
        {
            return new T2bEntryHeader
            {
                entryCount = br.ReadUInt32(),
                stringDataOffset = br.ReadUInt32(),
                stringDataLength = br.ReadUInt32(),
                stringDataCount = br.ReadUInt32()
            };
        }

        private bool TryDetectValueLength(IBinaryReaderX br, uint entryCount, long dataEndOffset, out ValueLength valueLength)
        {
            valueLength = default;

            long origPosition = br.BaseStream.Position;

            var valueLengths = new[] { ValueLength.Int, ValueLength.Long };
            foreach (ValueLength length in valueLengths)
            {
                if (TryReadEntrySection(br, entryCount, dataEndOffset, (int)length))
                {
                    br.BaseStream.Position = origPosition;
                    valueLength = length;

                    return true;
                }

                br.BaseStream.Position = origPosition;
            }

            return false;
        }

        private bool TryReadEntrySection(IBinaryReaderX br, uint entryCount, long dataEndOffset, int valueLength)
        {
            int[] valueLengths = { valueLength, valueLength, valueLength };

            for (var i = 0; i < entryCount; i++)
            {
                if (br.BaseStream.Position + 8 > dataEndOffset)
                    return false;

                br.BaseStream.Position += 4;

                int count = br.ReadByte();
                ValueType[] types = ReadEntryTypes(br, count);

                if (br.BaseStream.Position > dataEndOffset)
                    return false;

                if (types.Any(t => (int)t == 3))
                    return false;

                br.BaseStream.Position += types.Sum(t => valueLengths[(int)t]);
            }

            return br.BaseStream.Position <= dataEndOffset && dataEndOffset - br.BaseStream.Position < 0x10;
        }

        private T2bEntry[] ReadEntries(IBinaryReaderX br, uint entryCount, ValueLength valueLength)
        {
            var result = new T2bEntry[entryCount];

            for (var i = 0; i < entryCount; i++)
            {
                result[i] = new T2bEntry
                {
                    crc32 = br.ReadUInt32(),
                    entryCount = br.ReadByte()
                };

                result[i].entryTypes = ReadEntryTypes(br, result[i].entryCount);
                result[i].entryValues = ReadEntryValues(br, result[i].entryTypes, valueLength);
            }

            return result;
        }

        private ValueType[] ReadEntryTypes(IBinaryReaderX br, int count)
        {
            var types = new ValueType[count];

            for (var j = 0; j < types.Length; j += 4)
            {
                byte typeChunk = br.ReadByte();
                for (var h = 0; h < 4; h++)
                {
                    if (j + h >= types.Length)
                        break;

                    types[j + h] = (ValueType)((typeChunk >> h * 2) & 0x3);
                }
            }

            br.SeekAlignment(4);

            return types;
        }

        private long[] ReadEntryValues(IBinaryReaderX br, ValueType[] types, ValueLength valueLength)
        {
            var values = new long[types.Length];

            for (var j = 0; j < types.Length; j++)
            {
                switch (valueLength)
                {
                    case ValueLength.Int:
                        values[j] = br.ReadInt32();
                        break;

                    case ValueLength.Long:
                        values[j] = br.ReadInt64();
                        break;
                }
            }

            return values;
        }

        #endregion

        #region Checksums

        private T2bChecksumSection ReadChecksumSection(IBinaryReaderX br)
        {
            long sectionPosition = br.BaseStream.Position;

            T2bChecksumHeader checksumHeader = ReadChecksumHeader(br);
            long stringOffset = sectionPosition + checksumHeader.stringOffset;

            T2bChecksumEntry[] checksumEntries = Array.Empty<T2bChecksumEntry>();

            if (checksumHeader.count > 0)
                checksumEntries = ReadChecksumEntries(br, checksumHeader.count);

            return new T2bChecksumSection
            {
                Entries = checksumEntries,
                StringOffset = stringOffset,
                StringSize = (int)checksumHeader.stringSize
            };
        }

        private T2bChecksumHeader ReadChecksumHeader(IBinaryReaderX br)
        {
            return new T2bChecksumHeader
            {
                size = br.ReadUInt32(),
                count = br.ReadUInt32(),
                stringOffset = br.ReadUInt32(),
                stringSize = br.ReadUInt32()
            };
        }

        private bool TryDetectHashType(T2bChecksumEntry entry, byte[] stringData, T2bStringEncoding encoding, out HashType fileHashType)
        {
            fileHashType = default;

            string stringValue = ReadString(stringData, entry.stringOffset, encoding);

            var hashTypes = new[] { HashType.Crc32Standard, HashType.Crc32Jam };
            foreach (HashType hashType in hashTypes)
            {
                IChecksum<uint> checksum;
                switch (hashType)
                {
                    case HashType.Crc32Standard:
                        checksum = _checksumFactory.CreateCrc32();
                        break;

                    case HashType.Crc32Jam:
                        checksum = _checksumFactory.CreateCrc32Jam();
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown hash type '{hashType}'.");
                }

                uint stringHash = checksum.ComputeValue(stringValue);
                if (stringHash == entry.crc32)
                {
                    fileHashType = hashType;
                    return true;
                }
            }

            return false;
        }

        private T2bChecksumEntry[] ReadChecksumEntries(IBinaryReaderX br, uint count)
        {
            var result = new T2bChecksumEntry[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = new T2bChecksumEntry
                {
                    crc32 = br.ReadUInt32(),
                    stringOffset = br.ReadUInt32()
                };
            }

            return result;
        }

        #endregion

        private T2bFooter ReadFooter(IBinaryReaderX br)
        {
            return new T2bFooter
            {
                magic = br.ReadUInt32(),
                unk1 = br.ReadInt16(),
                encoding = br.ReadInt16(),
                unk2 = br.ReadInt16()
            };
        }

        private Contract.DataClasses.T2b CreateConfiguration(T2bEntrySection entrySection, T2bChecksumSection checksumSection,
            byte[] entryStringData, byte[] checksumStringData, T2bStringEncoding encoding, HashType hashType)
        {
            var checksumOffsetLookup = checksumSection.Entries.ToDictionary(x => x.crc32, y => y.stringOffset - checksumSection.Entries[0].stringOffset);

            var configEntries = new Contract.DataClasses.T2bEntry[entrySection.Entries.Length];
            for (var i = 0; i < entrySection.Entries.Length; i++)
            {
                var configEntryValues = new T2bEntryValue[entrySection.Entries[i].entryCount];
                for (var j = 0; j < entrySection.Entries[i].entryCount; j++)
                {
                    ValueType entryType = entrySection.Entries[i].entryTypes[j];
                    long entryValue = entrySection.Entries[i].entryValues[j];

                    object value;
                    switch (entryType)
                    {
                        case ValueType.String:
                            if (entryValue < 0)
                            {
                                value = string.Empty;
                                break;
                            }

                            value = ReadString(entryStringData, entryValue, encoding);
                            break;

                        case ValueType.Integer:
                            switch (entrySection.ValueLength)
                            {
                                case ValueLength.Int:
                                    value = (int)entryValue;
                                    break;

                                case ValueLength.Long:
                                    value = entryValue;
                                    break;

                                default:
                                    throw new InvalidOperationException($"Unknown value length {entrySection.ValueLength}.");
                            }
                            break;

                        case ValueType.FloatingPoint:
                            switch (entrySection.ValueLength)
                            {
                                case ValueLength.Int:
                                    value = BitConverter.Int32BitsToSingle((int)entryValue);
                                    break;

                                case ValueLength.Long:
                                    value = BitConverter.Int64BitsToDouble(entryValue);
                                    break;

                                default:
                                    throw new InvalidOperationException($"Unknown value length {entrySection.ValueLength}.");
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown value type {entryType} in config entry.");
                    }

                    configEntryValues[j] = new T2bEntryValue
                    {
                        Type = entryType,
                        Value = value
                    };
                }

                string name = ReadString(checksumStringData, checksumOffsetLookup[entrySection.Entries[i].crc32], encoding);

                configEntries[i] = new Contract.DataClasses.T2bEntry
                {
                    Name = name,
                    Values = configEntryValues
                };
            }

            return new Contract.DataClasses.T2b
            {
                Entries = configEntries,
                Encoding = GetStringEncoding(encoding),
                ValueLength = entrySection.ValueLength,
                HashType = hashType
            };
        }

        private string ReadString(byte[] stringData, long offset, T2bStringEncoding encoding)
        {
            long endOffset = offset;
            while (stringData[endOffset++] != 0) ;

            switch (encoding)
            {
                case T2bStringEncoding.Sjis:
                    return Encoding.GetEncoding("Shift-JIS").GetString(stringData[(int)offset..(int)(endOffset - 1)]);

                case T2bStringEncoding.Utf8:
                case T2bStringEncoding.Utf8_2:
                case T2bStringEncoding.Utf8_3:
                    return Encoding.UTF8.GetString(stringData[(int)offset..(int)(endOffset - 1)]);

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }

        private StringEncoding GetStringEncoding(T2bStringEncoding encoding)
        {
            switch (encoding)
            {
                case T2bStringEncoding.Sjis:
                    return StringEncoding.Sjis;

                case T2bStringEncoding.Utf8:
                case T2bStringEncoding.Utf8_2:
                case T2bStringEncoding.Utf8_3:
                    return StringEncoding.Utf8;

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }
    }
}
