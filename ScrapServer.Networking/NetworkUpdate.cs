﻿using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

public struct NetworkUpdate : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.NetworkUpdate;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public UInt32 GameTick;
    public byte[] Updates;

    public void Deserialize(ref BitReader reader)
    {
        GameTick = reader.ReadUInt32();
        Updates = new byte[reader.BytesLeft];
        reader.ReadBytes(Updates);
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(GameTick);
        writer.WriteBytes(Updates);
    }
}
