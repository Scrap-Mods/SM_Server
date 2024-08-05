using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;

public struct PlayerData : IBitSerializable
{
    public UInt32 CharacterID;
    public UInt64 SteamID;
    public UInt32 InventoryContainerID;
    public UInt32 CarryContainer;
    public UInt32 CarryColor;
    public string Name;
    public CharacterCustomization CharacterCustomization;

    public void Deserialize(ref BitReader reader)
    {
        CharacterID = reader.ReadUInt32();
        SteamID = reader.ReadUInt64();
        InventoryContainerID = reader.ReadUInt32();
        CarryContainer = reader.ReadUInt32();
        CarryColor = reader.ReadUInt32();
        reader.ReadByte();
        Name = reader.ReadString(reader.ReadByte());
        CharacterCustomization = reader.ReadObject<CharacterCustomization>();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(CharacterID);
        writer.WriteUInt64(SteamID);
        writer.WriteUInt32(InventoryContainerID);
        writer.WriteUInt32(CarryContainer);
        writer.WriteUInt32(CarryColor);
        writer.WriteByte(1);
        writer.WriteByte((byte)Name.Length);
        writer.WriteString(Name);
        writer.WriteObject(CharacterCustomization);
    }
}
