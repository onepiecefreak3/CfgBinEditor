using System.Buffers.Binary;
using Kaligraphy.Contract.DataClasses.Parsing;

namespace Logic.Foundation.PreviewManagement;

internal static class CharacterWireEnvelope
{
    public static byte[] Write(IList<CharacterData> characters)
    {
        var buffer = new List<byte>();
        WriteInt32(buffer, characters.Count);
        foreach (CharacterData character in characters)
        {
            if (character is not WireCharacterData wire)
                throw new InvalidOperationException("Native preview plugins only round-trip wire characters.");

            buffer.Add(wire.IsVisible ? (byte)1 : (byte)0);
            buffer.Add(wire.IsPersistent ? (byte)1 : (byte)0);
            WriteUInt32(buffer, wire.TypeId);
            WriteInt32(buffer, wire.Payload.Length);
            buffer.AddRange(wire.Payload);
        }

        return buffer.ToArray();
    }

    public static List<CharacterData> Read(ReadOnlySpan<byte> data)
    {
        int offset = 0;
        int count = ReadInt32(data, ref offset);
        var characters = new List<CharacterData>(count);
        for (int index = 0; index < count; index++)
        {
            bool isVisible = ReadUInt8(data, ref offset) != 0;
            bool isPersistent = ReadUInt8(data, ref offset) != 0;
            uint typeId = ReadUInt32(data, ref offset);
            int payloadLength = ReadInt32(data, ref offset);
            if (payloadLength < 0 || data.Length - offset < payloadLength)
                throw new InvalidOperationException("Truncated character payload.");

            byte[] payload = data.Slice(offset, payloadLength).ToArray();
            offset += payloadLength;
            characters.Add(new WireCharacterData(isVisible, isPersistent, typeId, payload));
        }

        return characters;
    }

    private static void WriteInt32(List<byte> buffer, int value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        buffer.AddRange(bytes.ToArray());
    }

    private static void WriteUInt32(List<byte> buffer, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        buffer.AddRange(bytes.ToArray());
    }

    private static byte ReadUInt8(ReadOnlySpan<byte> data, ref int offset)
    {
        if (data.Length - offset < 1)
            throw new InvalidOperationException("Truncated character wire.");

        return data[offset++];
    }

    private static int ReadInt32(ReadOnlySpan<byte> data, ref int offset)
    {
        if (data.Length - offset < 4)
            throw new InvalidOperationException("Truncated character wire.");

        int value = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, 4));
        offset += 4;
        return value;
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data, ref int offset)
    {
        if (data.Length - offset < 4)
            throw new InvalidOperationException("Truncated character wire.");

        uint value = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4));
        offset += 4;
        return value;
    }
}
