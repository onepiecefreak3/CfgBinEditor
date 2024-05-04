using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Rdbn.InternalContract;

namespace Logic.Domain.Level5Management
{
    internal class RdbnReader : IRdbnReader
    {
        private readonly IBinaryFactory _binaryFactory;

        private const int MinimumSize_ = 0x3C;

        public RdbnReader(IBinaryFactory binaryFactory)
        {
            _binaryFactory = binaryFactory;
        }

        public Contract.DataClasses.Rdbn? Read(Stream input)
        {
            using IBinaryReaderX br = _binaryFactory.CreateReader(input);

            // Read header
            if (br.BaseStream.Length < MinimumSize_)
                return null;

            RdbnHeader header = ReadHeader(br);
            if (header.magic != 0x4e424452)
                return null;

            int dataOffset = header.dataOffset << 2;

            // Read root entries
            br.BaseStream.Position = (header.rootOffset << 2) + dataOffset;
            RdbnRootEntry[] rootEntries = ReadRootEntries(br, header.rootCount);

            // Read type entries
            br.BaseStream.Position = (header.typeOffset << 2) + dataOffset;
            RdbnTypeEntry[] typeEntries = ReadTypeEntries(br, header.typeCount);

            // Read field entries
            br.BaseStream.Position = (header.fieldOffset << 2) + dataOffset;
            RdbnFieldEntry[] fieldEntries = ReadFieldEntries(br, header.fieldCount);

            // Read strings
            int hashOffset = (header.stringHashOffset << 2) + dataOffset;
            int offsetOffset = (header.stringOffsetsOffset << 2) + dataOffset;
            int stringOffset = header.stringOffset + dataOffset;
            IDictionary<uint, string>? stringLookup = ReadStrings(br, header.hashCount, hashOffset, offsetOffset, stringOffset);

            if (stringLookup == null)
                return null;

            int valueOffset = (header.valueOffset << 2) + dataOffset;
            return CreateRdbn(br, valueOffset, stringOffset, rootEntries, typeEntries, fieldEntries, stringLookup);
        }

        private RdbnHeader ReadHeader(IBinaryReaderX br)
        {
            var header = new RdbnHeader
            {
                magic = br.ReadUInt32(),
                headerSize = br.ReadInt16(),
                version = br.ReadInt32(),
                dataOffset = br.ReadInt16(),
                dataSize = br.ReadInt32()
            };

            br.BaseStream.Position += 0x14;

            header.typeOffset = br.ReadInt16();
            header.typeCount = br.ReadInt16();
            header.fieldOffset = br.ReadInt16();
            header.fieldCount = br.ReadInt16();
            header.rootOffset = br.ReadInt16();
            header.rootCount = br.ReadInt16();
            header.stringHashOffset = br.ReadInt16();
            header.stringOffsetsOffset = br.ReadInt16();
            header.hashCount = br.ReadInt16();
            header.valueOffset = br.ReadInt16();
            header.stringOffset = br.ReadInt32();

            return header;
        }

        private RdbnRootEntry[] ReadRootEntries(IBinaryReaderX br, int count)
        {
            var result = new RdbnRootEntry[count];

            long position = br.BaseStream.Position;
            for (var i = 0; i < count; i++)
            {
                result[i] = ReadRootEntry(br);
                br.BaseStream.Position = position + (i + 1) * 0x20;
            }

            return result;
        }

        private RdbnRootEntry ReadRootEntry(IBinaryReaderX br)
        {
            return new RdbnRootEntry
            {
                typeIndex = br.ReadInt16(),
                unk1 = br.ReadInt16(),
                valueOffset = br.ReadInt32(),
                valueSize = br.ReadInt32(),
                valueCount = br.ReadInt32(),
                nameHash = br.ReadUInt32()
            };
        }

        private RdbnTypeEntry[] ReadTypeEntries(IBinaryReaderX br, int count)
        {
            var result = new RdbnTypeEntry[count];

            long position = br.BaseStream.Position;
            for (var i = 0; i < count; i++)
            {
                result[i] = ReadTypeEntry(br);
                br.BaseStream.Position = position + (i + 1) * 0x20;
            }

            return result;
        }

        private RdbnTypeEntry ReadTypeEntry(IBinaryReaderX br)
        {
            return new RdbnTypeEntry
            {
                nameHash = br.ReadUInt32(),
                unk1 = br.ReadUInt32(),
                fieldIndex = br.ReadInt16(),
                fieldCount = br.ReadInt16()
            };
        }

        private RdbnFieldEntry[] ReadFieldEntries(IBinaryReaderX br, int count)
        {
            var result = new RdbnFieldEntry[count];

            long position = br.BaseStream.Position;
            for (var i = 0; i < count; i++)
            {
                result[i] = ReadFieldEntry(br);
                br.BaseStream.Position = position + (i + 1) * 0x20;
            }

            return result;
        }

        private RdbnFieldEntry ReadFieldEntry(IBinaryReaderX br)
        {
            return new RdbnFieldEntry
            {
                nameHash = br.ReadUInt32(),
                type = br.ReadInt16(),
                typeCategory = br.ReadInt16(),
                valueSize = br.ReadInt32(),
                valueOffset = br.ReadInt32(),
                valueCount = br.ReadInt32()
            };
        }

        private IDictionary<uint, string>? ReadStrings(IBinaryReaderX br, int count, int hashOffset, int offsetOffset, int stringOffset)
        {
            var hashes = new uint[count];
            var offsets = new int[count];

            br.BaseStream.Position = hashOffset;
            for (var i = 0; i < count; i++)
                hashes[i] = br.ReadUInt32();

            br.BaseStream.Position = offsetOffset;
            for (var i = 0; i < count; i++)
                offsets[i] = br.ReadInt32();

            var result = new Dictionary<uint, string>();

            for (var i = 0; i < count; i++)
            {
                if (stringOffset + offsets[i] > br.BaseStream.Length)
                    return null;

                br.BaseStream.Position = stringOffset + offsets[i];
                result[hashes[i]] = ReadString(br);
            }

            return result;
        }

        private string ReadString(IBinaryReaderX br)
        {
            var result = new List<byte>();

            byte byteValue = br.ReadByte();
            while (byteValue != 0)
            {
                result.Add(byteValue);
                byteValue = br.ReadByte();
            }

            return Encoding.ASCII.GetString(result.ToArray());
        }

        private Contract.DataClasses.Rdbn CreateRdbn(IBinaryReaderX br, int valueOffset, int stringOffset, RdbnRootEntry[] rootEntries, RdbnTypeEntry[] typeEntries, RdbnFieldEntry[] fieldEntries, IDictionary<uint, string> strings)
        {
            // Create type declarations
            var typeDeclarations = new RdbnTypeDeclaration[typeEntries.Length];

            for (var i = 0; i < typeEntries.Length; i++)
            {
                RdbnTypeEntry typeEntry = typeEntries[i];
                var fieldDeclarations = new RdbnFieldDeclaration[typeEntry.fieldCount];

                for (var j = 0; j < typeEntry.fieldCount; j++)
                {
                    RdbnFieldEntry fieldEntry = fieldEntries[typeEntry.fieldIndex + j];

                    fieldDeclarations[j] = new RdbnFieldDeclaration
                    {
                        Name = strings[fieldEntry.nameHash],
                        Count = fieldEntry.valueCount,
                        Size = fieldEntry.valueSize,
                        FieldType = (FieldType)fieldEntry.type,
                        FieldTypeCategory = (FieldTypeCategory)fieldEntry.typeCategory
                    };
                }

                typeDeclarations[i] = new RdbnTypeDeclaration
                {
                    Name = strings[typeEntry.nameHash],
                    UnkHash = typeEntry.unk1,
                    Fields = fieldDeclarations
                };
            }

            RdbnTypeDeclaration[] distinctTypeDeclarations = typeDeclarations.DistinctBy(x => x.GetHashCode()).ToArray();
            Dictionary<int, int> typeDeclarationLookup = distinctTypeDeclarations.Select((x, i) => (x, i)).ToDictionary(x => x.x.GetHashCode(), y => y.i);

            // Create lists
            var lists = new RdbnListEntry[rootEntries.Length];

            for (var i = 0; i < rootEntries.Length; i++)
            {
                RdbnRootEntry rootEntry = rootEntries[i];
                var listValues = new object[rootEntry.valueCount][][];

                int rootValueOffset = valueOffset + rootEntry.valueOffset;

                for (var j = 0; j < rootEntry.valueCount; j++)
                {
                    RdbnTypeEntry typeEntry = typeEntries[rootEntry.typeIndex];
                    listValues[j] = new object[typeEntry.fieldCount][];

                    int typeValueOffset = rootValueOffset + j * rootEntry.valueSize;

                    for (var h = 0; h < typeEntry.fieldCount; h++)
                    {
                        RdbnFieldEntry fieldEntry = fieldEntries[typeEntry.fieldIndex + h];
                        listValues[j][h] = new object[fieldEntry.valueCount];

                        br.BaseStream.Position = typeValueOffset + fieldEntry.valueOffset;

                        for (var k = 0; k < listValues[j][h].Length; k++)
                        {
                            switch (fieldEntry.type)
                            {
                                // Ability data
                                case 0:
                                    listValues[j][h][k] = br.ReadBytes(fieldEntry.valueSize);
                                    break;

                                // Enhance data
                                case 1:
                                    listValues[j][h][k] = br.ReadBytes(fieldEntry.valueSize);
                                    break;

                                // Status Rate
                                case 2:
                                    listValues[j][h][k] = br.ReadBytes(fieldEntry.valueSize);
                                    break;

                                // Bool
                                case 3:
                                    listValues[j][h][k] = br.ReadBoolean();
                                    break;

                                // Byte
                                case 4:
                                    listValues[j][h][k] = br.ReadByte();
                                    break;

                                // Short
                                case 5:
                                    listValues[j][h][k] = br.ReadInt16();
                                    break;

                                // Int
                                case 6:
                                    listValues[j][h][k] = br.ReadInt32();
                                    break;

                                // Act type
                                case 9:
                                    listValues[j][h][k] = br.ReadInt16();
                                    break;

                                // Flag
                                case 10:
                                    listValues[j][h][k] = br.ReadInt32();
                                    break;

                                // Float
                                case 0xD:
                                    listValues[j][h][k] = br.ReadSingle();
                                    break;

                                // Hash
                                case 0xF:
                                    listValues[j][h][k] = br.ReadUInt32();
                                    break;

                                // Rates
                                case 0x12:
                                    listValues[j][h][k] = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
                                    break;

                                // Position
                                case 0x13:
                                    listValues[j][h][k] = new[] { br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle() };
                                    break;

                                // Condition
                                case 0x14:
                                    uint conditionValue = br.ReadUInt32();

                                    if (stringOffset + conditionValue >= br.BaseStream.Length)
                                    {
                                        listValues[j][h][k] = conditionValue;
                                        break;
                                    }

                                    br.BaseStream.Position = stringOffset + conditionValue;
                                    listValues[j][h][k] = ReadString(br);
                                    break;

                                // Short tuple
                                case 0x15:
                                    listValues[j][h][k] = new[] { br.ReadInt16(), br.ReadInt16() };
                                    break;

                                default:
                                    throw new InvalidOperationException($"Invalid field type {fieldEntry.type}.");
                            }
                        }
                    }
                }

                lists[i] = new RdbnListEntry
                {
                    Name = strings[rootEntry.nameHash],
                    TypeIndex = typeDeclarationLookup[typeDeclarations[rootEntry.typeIndex].GetHashCode()],
                    Values = listValues
                };
            }

            return new Contract.DataClasses.Rdbn
            {
                Types = distinctTypeDeclarations,
                Lists = lists
            };
        }
    }
}
