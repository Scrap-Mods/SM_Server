using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;
using System.IO.Pipelines;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server during the join sequence containing general 
/// server info such as the game version, game mode, world seed, etc.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/server-info"/>
public struct ServerInfo : IBitSerializable
{
    /// <summary>
    /// The game version.
    /// </summary>
    public UInt32 Version;

    /// <summary>
    /// The game mode.
    /// </summary>
    public Gamemode Gamemode;

    /// <summary>
    /// The world seed.
    /// </summary>
    public UInt32 Seed;

    /// <summary>
    /// The current game tick.
    /// </summary>
    public UInt32 GameTick;

    /// <summary>
    /// The mods used on the server.
    /// </summary>
    public ModData[]? ModData;
    
    /// <summary>
    /// Purpose unknown.
    /// </summary>
    public byte[]? SomeData;

    /// <summary>
    /// Lua script data.
    /// </summary>
    public BlobDataRef[]? ScriptData;

    /// <summary>
    /// Generic game data (purpose not fully known).
    /// </summary>
    public BlobDataRef[]? GenericData;

    /// <summary>
    /// Server flags (dev mode).
    /// </summary>
    public ServerFlags Flags;

    /// <inheritdoc/>
    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)PacketId.ServerInfo);
        using var compWriter = writer.WriteLZ4();

        compWriter.Writer.WriteUInt32(Version);
        compWriter.Writer.WriteGamemode(Gamemode);
        compWriter.Writer.WriteUInt32(Seed);
        compWriter.Writer.WriteUInt32(GameTick);

        if (ModData == null)
        {
            compWriter.Writer.WriteUInt32(0);
        }
        else
        {
            compWriter.Writer.WriteUInt32((UInt32)ModData.Length);
            foreach (var modData in ModData)
            {
                writer.WriteObject(modData);
            }
        }

        if (SomeData == null)
        {
            compWriter.Writer.WriteUInt16(0);
        }
        else
        {
            compWriter.Writer.WriteUInt16((UInt16)SomeData.Length);
            compWriter.Writer.WriteBytes(SomeData);
        }

        if (ScriptData == null)
        {
            compWriter.Writer.WriteUInt32(0);
        }
        else
        {
            compWriter.Writer.WriteUInt32((UInt32)ScriptData.Length);
            foreach (var scriptData in ScriptData)
            {
                compWriter.Writer.WriteObject(scriptData);
            }
        }

        if (GenericData == null)
        {
            compWriter.Writer.WriteUInt32(0);
        }
        else
        {
            compWriter.Writer.WriteUInt32((UInt32)GenericData.Length);
            foreach (var generictData in GenericData)
            {
                compWriter.Writer.WriteObject(generictData);
            }
        }

        compWriter.Writer.WriteServerFlags(Flags);
    }

    /// <inheritdoc/>
    public void Deserialize(ref BitReader reader)
    {
        reader.ReadByte();
        using var compReader = reader.ReadLZ4();

        Version = compReader.Reader.ReadUInt32();
        Gamemode = compReader.Reader.ReadGamemode();
        Seed = compReader.Reader.ReadUInt32();
        GameTick = compReader.Reader.ReadUInt32();

        var modDataCount = compReader.Reader.ReadUInt32();
        ModData = new ModData[modDataCount];
        for (var i = 0; i < modDataCount; i++)
        {
            ModData[i] = compReader.Reader.ReadObject<ModData>();
        }

        var someDataCount = compReader.Reader.ReadUInt16();
        SomeData = new byte[someDataCount];
        compReader.Reader.ReadBytes(SomeData);

        var scriptDataCount = compReader.Reader.ReadUInt32();
        ScriptData = new BlobDataRef[scriptDataCount];
        for (var i = 0; i < scriptDataCount; i++)
        {
            ScriptData[i] = compReader.Reader.ReadObject<BlobDataRef>();
        }

        var genericDataCount = compReader.Reader.ReadUInt32();
        GenericData = new BlobDataRef[genericDataCount];
        for (var i = 0; i < genericDataCount; i++)
        {
            GenericData[i] = compReader.Reader.ReadObject<BlobDataRef>();
        }

        Flags = compReader.Reader.ReadServerFlags();
    }
}
