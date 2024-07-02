using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// The packet sent by the server during the join sequence containing general 
/// server info such as the game version, game mode, world seed, etc.
/// </summary>
/// <seealso href="https://docs.scrapmods.io/docs/networking/packets/server-info"/>
public struct ServerInfo : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ServerInfo;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

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
        writer.WriteUInt32(Version);
        writer.WriteGamemode(Gamemode);
        writer.WriteUInt32(Seed);
        writer.WriteUInt32(GameTick);

        if (ModData == null)
        {
            writer.WriteUInt32(0);
        }
        else
        {
            writer.WriteUInt32((UInt32)ModData.Length);
            foreach (var modData in ModData)
            {
                writer.WriteObject(modData);
            }
        }

        if (SomeData == null)
        {
            writer.WriteUInt32(0);
        }
        else
        {
            writer.WriteUInt32((UInt32)SomeData.Length);
            writer.WriteBytes(SomeData);
        }

        if (ScriptData == null)
        {
            writer.WriteUInt32(0);
        }
        else
        {
            writer.WriteUInt32((UInt32)ScriptData.Length);
            foreach (var scriptData in ScriptData)
            {
                writer.WriteObject(scriptData);
            }
        }

        if (GenericData == null)
        {
            writer.WriteUInt32(0);
        }
        else
        {
            writer.WriteUInt32((UInt32)GenericData.Length);
            foreach (var generictData in GenericData)
            {
                writer.WriteObject(generictData);
            }
        }

        writer.WriteServerFlags(Flags);
    }

    /// <inheritdoc/>
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
