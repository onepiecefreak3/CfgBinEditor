using System.Text;
using Logic.Domain.Kuriimu2.KomponentAdapter.Contract;
using Logic.Domain.Kuriimu2.KryptographyAdapter.Contract;
using Logic.Domain.Level5Management.Contract;
using Logic.Domain.Level5Management.Contract.DataClasses;
using Logic.Domain.Level5Management.Cryptography.InternalContract;
using Logic.Domain.Level5Management.Rdbn.InternalContract;

namespace Logic.Domain.Level5Management
{
    internal class RdbnWriter : IRdbnWriter
    {
        private const int DataOffset = 0x50;

        private readonly IBinaryFactory _binaryFactory;
        private readonly IChecksum<uint> _checksum;

        public RdbnWriter(IBinaryFactory binaryFactory, IChecksumFactory checksumFactory)
        {
            _binaryFactory = binaryFactory;
            _checksum = checksumFactory.CreateCrc32();
        }

        public Stream Write(Contract.DataClasses.Rdbn config)
        {
            RdbnTypeDeclaration[] distinctTypes = GetDistinctTypeDeclarations(config);

            int hashEntryCount = config.Lists.Length + distinctTypes.Length + distinctTypes.Sum(t => t.Fields.Length);
            int valueOffset = GetValueOffset(config, distinctTypes, hashEntryCount);

            RdbnTypeEntry[] typeEntries = GetTypeEntries(distinctTypes);
            RdbnFieldEntry[] fieldEntries = GetFieldEntries(distinctTypes);

            RdbnRootEntry[] rootEntries = GetRootEntries(config, distinctTypes, ref valueOffset);

            var output = new MemoryStream();
            using IBinaryWriterX bw = _binaryFactory.CreateWriter(output, true);

            output.Position = DataOffset;
            WriteTypes(bw, typeEntries);
            WriteFields(bw, fieldEntries);
            WriteLists(bw, rootEntries);

            int hashSize = hashEntryCount * 8;
            int valueSize = rootEntries[^1].valueOffset + rootEntries[^1].valueCount * rootEntries[^1].valueSize;

            long hashOffset = output.Position;
            long stringOffset = hashOffset + hashSize + valueSize;

            long localStringOffset = stringOffset;
            var stringCache = new Dictionary<string, long>();
            WriteTypeStrings(bw, distinctTypes, ref localStringOffset, stringCache);
            WriteValues(bw, config, rootEntries, typeEntries, fieldEntries, stringOffset, localStringOffset, valueOffset, stringCache);

            output.Position = hashOffset;
            WriteHashes(bw, config.Lists, distinctTypes, stringOffset, stringCache);

            output.Position = 0;
            WriteHeader(bw, rootEntries, typeEntries, fieldEntries, valueOffset, stringOffset, hashEntryCount);

            output.Position = 0;
            return output;
        }

        private RdbnTypeDeclaration[] GetDistinctTypeDeclarations(Contract.DataClasses.Rdbn config)
        {
            return config.Types.DistinctBy(x => x.GetHashCode()).ToArray();
        }

        private int GetValueOffset(Contract.DataClasses.Rdbn config, RdbnTypeDeclaration[] distinctTypes, int hashEntryCount)
        {
            int valueOffset = DataOffset;

            valueOffset += distinctTypes.Length * 0x20;
            valueOffset += distinctTypes.Sum(x => x.Fields.Length) * 0x20;
            valueOffset += config.Lists.Length * 0x20;

            valueOffset += hashEntryCount * 8;

            return valueOffset;
        }

        private RdbnTypeEntry[] GetTypeEntries(RdbnTypeDeclaration[] typeDeclarations)
        {
            var result = new RdbnTypeEntry[typeDeclarations.Length];

            short fieldIndex = 0;
            for (var i = 0; i < typeDeclarations.Length; i++)
            {
                result[i] = new RdbnTypeEntry
                {
                    nameHash = _checksum.ComputeValue(typeDeclarations[i].Name),
                    unk1 = typeDeclarations[i].UnkHash,
                    fieldIndex = fieldIndex,
                    fieldCount = (short)typeDeclarations[i].Fields.Length
                };

                fieldIndex += result[i].fieldCount;
            }

            return result;
        }

        private RdbnFieldEntry[] GetFieldEntries(RdbnTypeDeclaration[] typeDeclarations)
        {
            int totalFieldCount = typeDeclarations.Sum(x => x.Fields.Length);
            var result = new RdbnFieldEntry[totalFieldCount];

            var fieldIndex = 0;
            foreach (RdbnTypeDeclaration typeDeclaration in typeDeclarations)
            {
                var valueOffset = 0;
                foreach (RdbnFieldDeclaration fieldDeclaration in typeDeclaration.Fields)
                {
                    int valueSize = fieldDeclaration.Size;
                    if (fieldDeclaration.FieldTypeCategory != FieldTypeCategory.Special && (valueSize & (valueSize - 1)) == 0)
                        valueOffset = (valueOffset + (valueSize - 1)) & ~(valueSize - 1);

                    result[fieldIndex++] = new RdbnFieldEntry
                    {
                        nameHash = _checksum.ComputeValue(fieldDeclaration.Name),
                        type = (short)fieldDeclaration.FieldType,
                        typeCategory = (short)fieldDeclaration.FieldTypeCategory,
                        valueSize = valueSize,
                        valueOffset = valueOffset,
                        valueCount = fieldDeclaration.Count
                    };

                    valueOffset += fieldDeclaration.Count * valueSize;
                }
            }

            return result;
        }

        private RdbnRootEntry[] GetRootEntries(Contract.DataClasses.Rdbn config, RdbnTypeDeclaration[] distinctTypeDeclarations, ref int valueOffset)
        {
            Dictionary<int, int> typeLookup = distinctTypeDeclarations.Select((x, i) => (x.GetHashCode(), i)).ToDictionary(x => x.Item1, x => x.i);

            var result = new RdbnRootEntry[config.Lists.Length];

            int localValueOffset = valueOffset;
            int baseValueOffset = valueOffset;
            for (var i = 0; i < config.Lists.Length; i++)
            {
                RdbnListEntry rootEntry = config.Lists[i];

                RdbnTypeDeclaration type = config.Types[rootEntry.TypeIndex];
                var typeIndex = (short)typeLookup[type.GetHashCode()];

                int highestAlignment = GetHighestAlignment(type);
                localValueOffset = (localValueOffset + (highestAlignment - 1)) & ~(highestAlignment - 1);

                if (i == 0)
                    valueOffset = baseValueOffset = localValueOffset;

                result[i] = new RdbnRootEntry
                {
                    typeIndex = typeIndex,
                    unk1 = 2,
                    valueOffset = localValueOffset - baseValueOffset,
                    valueSize = GetTypeSize(type),
                    valueCount = rootEntry.Values.Length,
                    nameHash = _checksum.ComputeValue(rootEntry.Name)
                };

                localValueOffset += result[i].valueCount * result[i].valueSize;
            }

            return result;
        }

        private void WriteTypes(IBinaryWriterX bw, RdbnTypeEntry[] entries)
        {
            foreach (RdbnTypeEntry entry in entries)
            {
                bw.Write(entry.nameHash);
                bw.Write(entry.unk1);
                bw.Write(entry.fieldIndex);
                bw.Write(entry.fieldCount);

                bw.WritePadding(0x14);
            }
        }

        private void WriteFields(IBinaryWriterX bw, RdbnFieldEntry[] entries)
        {
            foreach (RdbnFieldEntry entry in entries)
            {
                bw.Write(entry.nameHash);
                bw.Write(entry.type);
                bw.Write(entry.typeCategory);
                bw.Write(entry.valueSize);
                bw.Write(entry.valueOffset);
                bw.Write(entry.valueCount);

                bw.WritePadding(0xC);
            }
        }

        private void WriteLists(IBinaryWriterX bw, RdbnRootEntry[] entries)
        {
            foreach (RdbnRootEntry entry in entries)
            {
                bw.Write(entry.typeIndex);
                bw.Write(entry.unk1);
                bw.Write(entry.valueOffset);
                bw.Write(entry.valueSize);
                bw.Write(entry.valueCount);
                bw.Write(entry.nameHash);

                bw.WritePadding(0xC);
            }
        }

        private void WriteHashes(IBinaryWriterX bw, RdbnListEntry[] lists, RdbnTypeDeclaration[] types, long baseStringOffset, IDictionary<string, long> stringCache)
        {
            // Write hashes
            foreach (RdbnListEntry list in lists)
                bw.Write(_checksum.ComputeValue(list.Name));

            foreach (RdbnTypeDeclaration type in types)
            {
                bw.Write(_checksum.ComputeValue(type.Name));

                foreach (RdbnFieldDeclaration field in type.Fields)
                    bw.Write(_checksum.ComputeValue(field.Name));
            }

            // Write offsets
            foreach (RdbnListEntry list in lists)
                bw.Write((int)(stringCache[list.Name] - baseStringOffset));

            foreach (RdbnTypeDeclaration type in types)
            {
                bw.Write((int)(stringCache[type.Name] - baseStringOffset));

                foreach (RdbnFieldDeclaration field in type.Fields)
                    bw.Write((int)(stringCache[field.Name] - baseStringOffset));
            }
        }

        private void WriteTypeStrings(IBinaryWriterX bw, RdbnTypeDeclaration[] types, ref long stringOffset, IDictionary<string, long> stringCache)
        {
            foreach (RdbnTypeDeclaration type in types)
            {
                if (!stringCache.ContainsKey(type.Name))
                    stringOffset = WriteString(bw, type.Name, stringOffset, stringCache);

                foreach (RdbnFieldDeclaration field in type.Fields)
                {
                    if (stringCache.ContainsKey(field.Name))
                        continue;

                    stringOffset = WriteString(bw, field.Name, stringOffset, stringCache);
                }
            }
        }

        private void WriteValues(IBinaryWriterX bw, Contract.DataClasses.Rdbn config, RdbnRootEntry[] lists, RdbnTypeEntry[] types, RdbnFieldEntry[] fields,
            long baseStringOffset, long stringOffset, long valueOffset, IDictionary<string, long> stringCache)
        {
            bw.BaseStream.Position = valueOffset;

            for (var i = 0; i < lists.Length; i++)
            {
                RdbnRootEntry list = lists[i];
                RdbnTypeEntry type = types[list.typeIndex];

                if (!stringCache.ContainsKey(config.Lists[i].Name))
                    stringOffset = WriteString(bw, config.Lists[i].Name, stringOffset, stringCache);

                object[][][] listValues = config.Lists[i].Values;

                for (var j = 0; j < list.valueCount; j++)
                {
                    object[][] typeValues = listValues[j];

                    for (var h = 0; h < type.fieldCount; h++)
                    {
                        RdbnFieldEntry field = fields[type.fieldIndex + h];

                        object[] fieldValues = typeValues[h];

                        for (var k = 0; k < field.valueCount; k++)
                        {
                            bw.BaseStream.Position = valueOffset + list.valueOffset + j * list.valueSize + field.valueOffset;
                            WriteValue(bw, fieldValues[k], field, baseStringOffset, ref stringOffset, stringCache);
                        }
                    }
                }
            }
        }

        private void WriteValue(IBinaryWriterX bw, object value, RdbnFieldEntry field, long baseStringOffset, ref long stringOffset,
            IDictionary<string, long> stringCache)
        {
            // Sanity check
            if (field.typeCategory != (short)FieldTypeCategory.Special && (field.valueSize & (field.valueSize - 1)) == 0)
                if ((bw.BaseStream.Position & (field.valueSize - 1)) != 0)
                    throw new InvalidOperationException("Alignment of values was not calculated correctly.");

            switch (field.type)
            {
                // Ability data
                case 0:
                    bw.Write((byte[])value);
                    break;

                // Enhance data
                case 1:
                    bw.Write((byte[])value);
                    break;

                // Status Rate
                case 2:
                    bw.Write((byte[])value);
                    break;

                // Bool
                case 3:
                    bw.Write((bool)value);
                    break;

                // Byte
                case 4:
                    bw.Write((byte)value);
                    break;

                // Short
                case 5:
                    bw.Write((short)value);
                    break;

                // Int
                case 6:
                    bw.Write((int)value);
                    break;

                // Act type
                case 9:
                    bw.Write((short)value);
                    break;

                // Flag
                case 10:
                    bw.Write((int)value);
                    break;

                // Float
                case 0xD:
                    bw.Write((float)value);
                    break;

                // Hash
                case 0xF:
                    bw.Write((uint)value);
                    break;

                // Rates
                case 0x12:
                    for (var i = 0; i < ((float[])value).Length; i++)
                        bw.Write(((float[])value)[i]);
                    break;

                // Position
                case 0x13:
                    for (var i = 0; i < ((float[])value).Length; i++)
                        bw.Write(((float[])value)[i]);
                    break;

                // Condition
                case 0x14:
                    if (value is uint uValue)
                    {
                        bw.Write(uValue);
                        break;
                    }

                    if (!stringCache.TryGetValue((string)value, out long sValue))
                    {
                        sValue = stringOffset;

                        stringOffset = WriteString(bw, (string)value, stringOffset, stringCache);
                    }

                    bw.Write((int)(sValue - baseStringOffset));
                    break;

                // Short tuple
                case 0x15:
                    for (var i = 0; i < ((short[])value).Length; i++)
                        bw.Write(((short[])value)[i]);
                    break;

                default:
                    throw new InvalidOperationException($"Invalid field type {field.type}.");
            }
        }

        private void WriteHeader(IBinaryWriterX bw, RdbnRootEntry[] lists, RdbnTypeEntry[] types, RdbnFieldEntry[] fields, long valueOffset, long stringOffset, int hashEntryCount)
        {
            bw.Write(0x4e424452);
            bw.Write((short)DataOffset);
            bw.Write(0x64);
            bw.Write((short)(DataOffset >> 2));
            bw.Write((int)bw.BaseStream.Length - DataOffset);

            bw.BaseStream.Position += 0x14;

            var offset = 0;
            bw.Write((short)offset);
            bw.Write((short)types.Length);

            offset += types.Length * 0x20;
            bw.Write((short)(offset >> 2));
            bw.Write((short)fields.Length);

            offset += fields.Length * 0x20;
            bw.Write((short)(offset >> 2));
            bw.Write((short)lists.Length);

            offset += lists.Length * 0x20;
            bw.Write((short)(offset >> 2));

            offset += hashEntryCount * 4;
            bw.Write((short)(offset >> 2));

            bw.Write((short)hashEntryCount);

            bw.Write((short)((valueOffset - DataOffset) >> 2));
            bw.Write(stringOffset - DataOffset);
        }

        private int GetTypeSize(RdbnTypeDeclaration type)
        {
            var highestAlignment = 1;

            var valueOffset = 0;
            foreach (RdbnFieldDeclaration field in type.Fields)
            {
                int valueSize = field.Size;
                if (field.FieldTypeCategory != FieldTypeCategory.Special && (valueSize & (valueSize - 1)) == 0)
                {
                    valueOffset = (valueOffset + (valueSize - 1)) & ~(valueSize - 1);
                    if (highestAlignment < valueSize)
                        highestAlignment = valueSize;
                }

                valueOffset += field.Count * valueSize;
            }

            return (valueOffset + (highestAlignment - 1)) & ~(highestAlignment - 1);
        }

        private int GetHighestAlignment(RdbnTypeDeclaration type)
        {
            var highestAlignment = 1;

            foreach (RdbnFieldDeclaration field in type.Fields)
            {
                if (field.FieldTypeCategory != FieldTypeCategory.Special && (field.Size & (field.Size - 1)) == 0 && highestAlignment < field.Size)
                    highestAlignment = field.Size;
            }

            return highestAlignment;
        }

        private long WriteString(IBinaryWriterX bw, string value, long offset, IDictionary<string, long> writtenNames)
        {
            CacheStrings(offset, value, Encoding.ASCII, writtenNames);

            long origPosition = bw.BaseStream.Position;
            bw.BaseStream.Position = offset;

            bw.WriteString(value, Encoding.ASCII, false);
            offset = bw.BaseStream.Position;

            bw.BaseStream.Position = origPosition;

            return offset;
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
    }
}
