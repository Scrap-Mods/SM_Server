using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct ServerInfo : IPacket
{
    public static PacketType PacketId => PacketType.ServerInfo;
    public static bool IsCompressable => true;

    public UInt32 Version { get; set; }
    public Gamemode Gamemode { get; set; }
    public UInt32 Seed { get; set; }
    public UInt32 GameTick { get; set; }
    public ModData[] ModData { get; set; }
    public byte[] SomeData { get; set; }
    public GenericData[] ScriptData { get; set; }
    public GenericData[] GenericData { get; set; }
    public ServerFlags Flags { get; set; }

    public ServerInfo()
    {
        ModData = Array.Empty<ModData>();
        SomeData = Array.Empty<byte>();
        ScriptData = Array.Empty<GenericData>();
        GenericData = Array.Empty<GenericData>();
    }

    public ServerInfo(
        UInt32 version,
        Gamemode gamemode,
        UInt32 seed,
        UInt32 gameTick,
        ModData[] modData,
        byte[] someData,
        GenericData[] scriptData,
        GenericData[] genericData,
        ServerFlags flags)
    {
        Version = version;
        Gamemode = gamemode;
        Seed = seed;
        GameTick = gameTick;
        ModData = modData;
        SomeData = someData;
        ScriptData = scriptData;
        GenericData = genericData;
        Flags = flags;
    }

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
        ScriptData = new GenericData[scriptDataCount];
        for (var i = 0; i < scriptDataCount; i++)
        {
            ScriptData[i] = reader.ReadObject<GenericData>();
        }

        var genericDataCount = reader.ReadUInt32();
        GenericData = new GenericData[genericDataCount];
        for (var i = 0; i < genericDataCount; i++)
        {
            GenericData[i] = reader.ReadObject<GenericData>();
        }

        Flags = reader.ReadServerFlags();
    }
}
