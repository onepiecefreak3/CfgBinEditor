using Logic.Domain.Level5.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Domain.Level5.Contract.DataClasses;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5.ConfigBinary.InternalContract.DataClasses;
using ValueType = Logic.Domain.Level5.Contract.DataClasses.ValueType;

namespace Logic.Domain.Level5
{
    internal class ConfigurationReader : IConfigurationReader
    {
        private readonly IBinaryFactory _binaryFactory;

        public ConfigurationReader(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public Configuration Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input);

            CfgBinHeader header = ReadHeader(br);
            CfgBinEntry[] entries = ReadEntries(br, header.entryCount);

            long checksumLookupOffset = (header.dataOffset + header.dataLength + 0xF) & ~0xF;
            br.BaseStream.Position = checksumLookupOffset;

            CfgBinChecksumHeader checksumHeader = ReadChecksumHeader(br);
            CfgBinChecksumEntry[] checksumEntries = ReadChecksumEntries(br, checksumHeader.count);

            long footerOffset = checksumLookupOffset + checksumHeader.size;
            br.BaseStream.Position = footerOffset;

            CfgBinFooter footer = ReadFooter(br);
            var encoding = (CfgBinStringEncoding)footer.encoding;

            return CreateConfiguration(br, entries, checksumEntries, header.dataOffset, checksumLookupOffset + checksumHeader.stringOffset, encoding);
        }

        private CfgBinHeader ReadHeader(IBinaryReaderX br)
        {
            return new CfgBinHeader
            {
                entryCount = br.ReadUInt32(),
                dataOffset = br.ReadUInt32(),
                dataLength = br.ReadUInt32(),
                dataCount = br.ReadUInt32()
            };
        }

        private CfgBinEntry[] ReadEntries(IBinaryReaderX br, uint entryCount)
        {
            var result = new CfgBinEntry[entryCount];

            for (var i = 0; i < entryCount; i++)
            {
                result[i] = new CfgBinEntry
                {
                    crc32 = br.ReadUInt32(),
                    entryCount = br.ReadByte()
                };

                var types = new byte[result[i].entryCount];
                for (var j = 0; j < types.Length; j += 4)
                {
                    byte typeChunk = br.ReadByte();
                    for (var h = 0; h < 4; h++)
                    {
                        if (j + h >= types.Length)
                            break;

                        types[j + h] = (byte)((typeChunk >> h * 2) & 0x3);
                    }
                }

                br.SeekAlignment(4);

                var values = new uint[result[i].entryCount];
                for (var j = 0; j < types.Length; j++)
                    values[j] = br.ReadUInt32();

                result[i].entryTypes = types;
                result[i].entryValues = values;
            }

            return result;
        }

        private CfgBinChecksumHeader ReadChecksumHeader(IBinaryReaderX br)
        {
            return new CfgBinChecksumHeader
            {
                size = br.ReadUInt32(),
                count = br.ReadInt32(),
                stringOffset = br.ReadUInt32(),
                stringSize = br.ReadInt32()
            };
        }

        private CfgBinChecksumEntry[] ReadChecksumEntries(IBinaryReaderX br, int count)
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

        private Configuration CreateConfiguration(IBinaryReaderX br,
            CfgBinEntry[] entries, CfgBinChecksumEntry[] checksumEntries,
            long stringOffset, long checksumStringOffset,
            CfgBinStringEncoding encoding)
        {
            var checksumOffsetLookup = checksumEntries.ToDictionary(x => x.crc32, y => y.stringOffset);

            var configEntries = new ConfigurationEntry[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var configEntryValues = new ConfigurationEntryValue[entries[i].entryCount];
                for (var j = 0; j < entries[i].entryCount; j++)
                {
                    var type = (ValueType)entries[i].entryTypes[j];

                    var intValue = unchecked((int)entries[i].entryValues[j]);
                    object value;
                    switch (type)
                    {
                        case ValueType.String:
                            if (intValue < 0)
                            {
                                value = string.Empty;
                                break;
                            }

                            br.BaseStream.Position = stringOffset + intValue;
                            value = ReadString(br, encoding);
                            break;

                        case ValueType.UInt:
                            value = unchecked((uint)intValue);
                            break;

                        case ValueType.Float:
                            value = BitConverter.Int32BitsToSingle(intValue);
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown value type {type} in config entry.");
                    }

                    configEntryValues[j] = new ConfigurationEntryValue
                    {
                        Type = type,
                        Value = value
                    };
                }

                br.BaseStream.Position = checksumStringOffset + checksumOffsetLookup[entries[i].crc32];
                string name = ReadString(br, encoding);

                configEntries[i] = new ConfigurationEntry
                {
                    Name = name,
                    Values = configEntryValues
                };
            }

            return new Configuration
            {
                Entries = configEntries
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
                    return Encoding.UTF8.GetString(result.ToArray());

                default:
                    throw new InvalidOperationException($"Unknown encoding {encoding}.");
            }
        }
    }
}
