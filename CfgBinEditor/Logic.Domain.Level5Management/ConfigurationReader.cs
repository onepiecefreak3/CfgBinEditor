using Logic.Domain.Level5.Contract;
using System.Text;
using Logic.Domain.Level5.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses;
using Logic.Domain.Level5.Cryptography.InternalContract;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5
{
    internal class ConfigurationReader : IConfigurationReader
    {
        private readonly IBinaryFactory _binaryFactory;
        private readonly IChecksumFactory _checksumFactory;

        private const int MinimumSize_ = 0x30;

        public ConfigurationReader(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksumFactory = checksumFactory;
        }

        public Configuration? Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input);

            // Read cfg.bin footer
            if (br.BaseStream.Length < MinimumSize_)
                return null;

            br.BaseStream.Position = input.Length - 0x10;

            CfgBinFooter footer = ReadFooter(br);
            if (footer.magic != 0x62327401)
                return null;

            // Read cfg.bin entry section
            br.BaseStream.Position = 0;

            CfgBinEntrySection? entrySection = ReadEntrySection(br);
            if (entrySection == null)
                return null;

            // Read cfg.bin checksum section
            var encoding = (CfgBinStringEncoding)footer.encoding;

            CfgBinChecksumSection? checksumSection = ReadChecksumSection(br, encoding);
            if (checksumSection == null)
                return null;

            return CreateConfiguration(br, entrySection.Value, checksumSection.Value, encoding);
        }

        #region Entries

        private CfgBinEntrySection? ReadEntrySection(IBinaryReaderX br)
        {
            long sectionPosition = br.BaseStream.Position;

            CfgBinEntryHeader entryHeader = ReadEntryHeader(br);
            long stringOffset = sectionPosition + entryHeader.stringDataOffset;

            ValueLength valueLength = default;
            CfgBinEntry[] entries = Array.Empty<CfgBinEntry>();

            if (entryHeader.entryCount > 0)
            {
                if (!TryDetectValueLength(br, entryHeader.entryCount, stringOffset, out valueLength))
                    return null;

                entries = ReadEntries(br, entryHeader.entryCount, valueLength);
            }

            br.BaseStream.Position = Math.Max(0x10, (stringOffset + entryHeader.stringDataLength + 15) & ~15);

            return new CfgBinEntrySection
            {
                Entries = entries,
                StringOffset = stringOffset,
                ValueLength = valueLength
            };
        }

        private CfgBinEntryHeader ReadEntryHeader(IBinaryReaderX br)
        {
            return new CfgBinEntryHeader
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

        private CfgBinEntry[] ReadEntries(IBinaryReaderX br, uint entryCount, ValueLength valueLength)
        {
            var result = new CfgBinEntry[entryCount];

            for (var i = 0; i < entryCount; i++)
            {
                result[i] = new CfgBinEntry
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

        private CfgBinChecksumSection? ReadChecksumSection(IBinaryReaderX br, CfgBinStringEncoding encoding)
        {
            long sectionPosition = br.BaseStream.Position;

            CfgBinChecksumHeader checksumHeader = ReadChecksumHeader(br);
            long stringOffset = sectionPosition + checksumHeader.stringOffset;

            HashType hashType = default;
            CfgBinChecksumEntry[] checksumEntries = Array.Empty<CfgBinChecksumEntry>();

            if (checksumHeader.count > 0)
            {
                if (!TryDetectHashType(br, stringOffset, encoding, out hashType))
                    return null;

                checksumEntries = ReadChecksumEntries(br, checksumHeader.count);
            }

            br.BaseStream.Position = Math.Max(0x10, (stringOffset + checksumHeader.stringSize + 15) & ~15);

            return new CfgBinChecksumSection
            {
                Entries = checksumEntries,
                StringOffset = stringOffset,
                HashType = hashType
            };
        }

        private CfgBinChecksumHeader ReadChecksumHeader(IBinaryReaderX br)
        {
            return new CfgBinChecksumHeader
            {
                size = br.ReadUInt32(),
                count = br.ReadUInt32(),
                stringOffset = br.ReadUInt32(),
                stringSize = br.ReadUInt32()
            };
        }

        private bool TryDetectHashType(IBinaryReaderX br, long stringOffset, CfgBinStringEncoding encoding, out HashType fileHashType)
        {
            fileHashType = default;

            long origPosition = br.BaseStream.Position;

            CfgBinChecksumEntry checksumEntry = ReadChecksumEntries(br, 1)[0];

            br.BaseStream.Position = stringOffset; // HINT: Assume string offset 0 for first entry
            string stringValue = ReadString(br, encoding);

            br.BaseStream.Position = origPosition;

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
                if (stringHash == checksumEntry.crc32)
                {
                    fileHashType = hashType;
                    return true;
                }
            }

            return false;
        }

        private CfgBinChecksumEntry[] ReadChecksumEntries(IBinaryReaderX br, uint count)
        {
            var result = new CfgBinChecksumEntry[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = new CfgBinChecksumEntry
                {
                    crc32 = br.ReadUInt32(),
                    stringOffset = br.ReadUInt32()
                };
            }

            return result;
        }

        #endregion

        private CfgBinFooter ReadFooter(IBinaryReaderX br)
        {
            return new CfgBinFooter
            {
                magic = br.ReadUInt32(),
                unk1 = br.ReadInt16(),
                encoding = br.ReadInt16(),
                unk2 = br.ReadInt16()
            };
        }

        private Configuration CreateConfiguration(IBinaryReaderX br, CfgBinEntrySection entrySection, CfgBinChecksumSection checksumSection, CfgBinStringEncoding encoding)
        {
            var checksumOffsetLookup = checksumSection.Entries.ToDictionary(x => x.crc32, y => y.stringOffset - checksumSection.Entries[0].stringOffset);

            var configEntries = new ConfigurationEntry[entrySection.Entries.Length];
            for (var i = 0; i < entrySection.Entries.Length; i++)
            {
                var configEntryValues = new ConfigurationEntryValue[entrySection.Entries[i].entryCount];
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

                            br.BaseStream.Position = entrySection.StringOffset + entryValue;
                            value = ReadString(br, encoding);
                            break;

                        case ValueType.Long:
                            value = entryValue;
                            break;

                        case ValueType.Double:
                            switch (entrySection.ValueLength)
                            {
                                case ValueLength.Int:
                                    value = (double)BitConverter.Int32BitsToSingle((int)entryValue);
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

                    configEntryValues[j] = new ConfigurationEntryValue
                    {
                        Type = entryType,
                        Value = value
                    };
                }

                br.BaseStream.Position = checksumSection.StringOffset + checksumOffsetLookup[entrySection.Entries[i].crc32];
                string name = ReadString(br, encoding);

                configEntries[i] = new ConfigurationEntry
                {
                    Name = name,
                    Values = configEntryValues
                };
            }

            return new Configuration
            {
                Entries = configEntries,
                Encoding = GetStringEncoding(encoding),
                ValueLength = entrySection.ValueLength,
                HashType = checksumSection.HashType
            };
        }

        private string ReadString(IBinaryReaderX br, CfgBinStringEncoding encoding)
        {
            var result = new List<byte>();

            byte byteValue = br.ReadByte();
            while (byteValue != 0)
            {
                result.Add(byteValue);
                byteValue = br.ReadByte();
            }

            switch (encoding)
            {
                case CfgBinStringEncoding.Sjis:
                    return Encoding.GetEncoding("Shift-JIS").GetString(result.ToArray());

                case CfgBinStringEncoding.Utf8:
                case CfgBinStringEncoding.Utf8_2:
                case CfgBinStringEncoding.Utf8_3:
                    return Encoding.UTF8.GetString(result.ToArray());

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }

        private StringEncoding GetStringEncoding(CfgBinStringEncoding encoding)
        {
            switch (encoding)
            {
                case CfgBinStringEncoding.Sjis:
                    return StringEncoding.Sjis;

                case CfgBinStringEncoding.Utf8:
                case CfgBinStringEncoding.Utf8_2:
                case CfgBinStringEncoding.Utf8_3:
                    return StringEncoding.Utf8;

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }
    }
}
