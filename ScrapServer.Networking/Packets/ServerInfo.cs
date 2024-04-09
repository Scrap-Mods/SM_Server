using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public class ServerInfo : IPacket
{
    public static PacketType PacketId => PacketType.ServerInfo;

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

    public void Serialize(ref BitWriter writer)
    {
        writer.WritePacketType(PacketId);

        using var comp = writer.WriteLZ4();

        comp.Writer.WriteUInt32(Version);
        comp.Writer.WriteGamemode(Gamemode);
        comp.Writer.WriteUInt32(Seed);
        comp.Writer.WriteUInt32(GameTick);

        comp.Writer.WriteUInt32((UInt32)ModData.Length);
        foreach (var modData in ModData)
        {
            comp.Writer.WriteObject(modData);
        }

        comp.Writer.WriteUInt32((UInt32)SomeData.Length);
        comp.Writer.WriteBytes(SomeData);

        comp.Writer.WriteUInt32((UInt32)ScriptData.Length);
        foreach (var scriptData in ScriptData)
        {
            comp.Writer.WriteObject(scriptData);
        }

        comp.Writer.WriteUInt32((UInt32)GenericData.Length);
        foreach (var generictData in GenericData)
        {
            comp.Writer.WriteObject(generictData);
        }

        comp.Writer.WriteServerFlags(Flags);
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadPacketType();

        using var decomp = reader.ReadLZ4(reader.BytesLeft);

        Version = decomp.Reader.ReadUInt32();
        Gamemode = decomp.Reader.ReadGamemode();
        Seed = decomp.Reader.ReadUInt32();
        GameTick = decomp.Reader.ReadUInt32();

        var modDataCount = decomp.Reader.ReadUInt32();
        ModData = new ModData[modDataCount];
        for (var i = 0; i < modDataCount; i++)
        {
            ModData[i] = decomp.Reader.ReadObject<ModData>();
        }

        var someDataCount = decomp.Reader.ReadUInt32();
        SomeData = new byte[someDataCount];
        decomp.Reader.ReadBytes(SomeData);

        var scriptDataCount = decomp.Reader.ReadUInt32();
        ScriptData = new GenericData[scriptDataCount];
        for (var i = 0; i < scriptDataCount; i++)
        {
            ScriptData[i] = decomp.Reader.ReadObject<GenericData>();
        }

        var genericDataCount = decomp.Reader.ReadUInt32();
        GenericData = new GenericData[genericDataCount];
        for (var i = 0; i < genericDataCount; i++)
        {
            GenericData[i] = decomp.Reader.ReadObject<GenericData>();
        }

        Flags = decomp.Reader.ReadServerFlags();
    }
}
