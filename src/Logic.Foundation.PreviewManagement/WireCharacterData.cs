using Kaligraphy.Contract.DataClasses.Parsing;

namespace Logic.Foundation.PreviewManagement;

internal sealed class WireCharacterData : CharacterData
{
    public uint TypeId { get; }

    public byte[] Payload { get; }

    public WireCharacterData(bool isVisible, bool isPersistent, uint typeId, byte[] payload)
    {
        IsVisible = isVisible;
        IsPersistent = isPersistent;
        TypeId = typeId;
        Payload = payload;
    }

    public WireCharacterData WithFlags(bool isVisible, bool isPersistent)
    {
        return new WireCharacterData(isVisible, isPersistent, TypeId, Payload);
    }
}
