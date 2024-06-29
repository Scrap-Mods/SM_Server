using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ServerInfo : IPacket
{
    public static PacketId PacketId => PacketId.ServerInfo;
    public static bool IsCompressable => true;

    public UInt32 Version;
    public Gamemode Gamemode;
    public UInt32 Seed;
    public UInt32 GameTick;
    public ModData[] ModData;
    public byte[] SomeData;
    public BlobDataRef[] ScriptData;
    public BlobDataRef[] GenericData;
    public ServerFlags Flags;

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Version);
        writer.WriteGamemode(Gamemode);
        writer.WriteUInt32(Seed);
        writer.WriteUInt32(GameTick);

        writer.WriteUInt32((UInt32)ModData.Length);
        foreach (var modData in ModData)
        {
            writer.WriteObject(modData);
        }

        writer.WriteUInt32((UInt32)SomeData.Length);
        writer.WriteBytes(SomeData);

        writer.WriteUInt32((UInt32)ScriptData.Length);
        foreach (var scriptData in ScriptData)
        {
            writer.WriteObject(scriptData);
        }

        writer.WriteUInt32((UInt32)GenericData.Length);
        foreach (var generictData in GenericData)
        {
            writer.WriteObject(generictData);
        }

        writer.WriteServerFlags(Flags);
    }

    public void Deserialize(ref BitReader reader)
    {
        Version = reader.ReadUInt32();
        Gamemode = reader.ReadGamemode();
        Seed = reader.ReadUInt32();
        GameTick = reader.ReadUInt32();

        var modDataCount = reader.ReadUInt32();
        ModData = new ModData[modDataCount];
        for (var i = 0; i < modDataCount; i++)
        {
            ModData[i] = reader.ReadObject<ModData>();
        }

        var someDataCount = reader.ReadUInt32();
        SomeData = new byte[someDataCount];
        reader.ReadBytes(SomeData);

        var scriptDataCount = reader.ReadUInt32();
        ScriptData = new BlobDataRef[scriptDataCount];
        for (var i = 0; i < scriptDataCount; i++)
        {
            ScriptData[i] = reader.ReadObject<BlobDataRef>();
        }

        var genericDataCount = reader.ReadUInt32();
        GenericData = new BlobDataRef[genericDataCount];
        for (var i = 0; i < genericDataCount; i++)
        {
            GenericData[i] = reader.ReadObject<BlobDataRef>();
        }

        Flags = reader.ReadServerFlags();
    }
}
