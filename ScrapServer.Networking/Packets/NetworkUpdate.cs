﻿using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct NetworkUpdate : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.NetworkUpdate;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public UInt32 GameTick;
    public NetObj[] Updates;

    public void Deserialize(ref BitReader reader)
    {
        GameTick = reader.ReadUInt32();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(GameTick);
        writer.WriteBytes(Data);
    }
}
