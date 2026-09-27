using System.Buffers.Binary;
using Shouldly;
using Xunit;

namespace Logic.Foundation.PreviewManagement.Tests;

public class CharacterWireEnvelopeTests
{
    [Fact]
    public void WithFlags_ClearsVisible_KeepsPayload()
    {
        byte[] payload = [9, 8, 7];
        var character = new WireCharacterData(true, true, 0xA1B2C3D4, payload);
        WireCharacterData edited = character.WithFlags(false, true);

        byte[] bytes = CharacterWireEnvelope.Write([edited]);

        BinaryPrimitives.ReadInt32LittleEndian(bytes).ShouldBe(1);
        bytes[4].ShouldBe((byte)0);
        bytes[5].ShouldBe((byte)1);
        BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(6)).ShouldBe(0xA1B2C3D4u);
        BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(10)).ShouldBe(payload.Length);
        bytes.AsSpan(14).ToArray().ShouldBe(payload);

        WireCharacterData read = CharacterWireEnvelope.Read(bytes).ShouldHaveSingleItem().ShouldBeOfType<WireCharacterData>();
        read.IsVisible.ShouldBeFalse();
        read.IsPersistent.ShouldBeTrue();
        read.TypeId.ShouldBe(0xA1B2C3D4u);
        read.Payload.ShouldBe(payload);
    }
}
