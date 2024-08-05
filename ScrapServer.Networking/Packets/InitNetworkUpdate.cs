﻿using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct InitNetworkUpdate : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.InitNetworkUpdate;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public NetworkUpdate Update;

    public void Deserialize(ref BitReader reader)
    {
        Update.Deserialize(ref reader);
    }

    public void Serialize(ref BitWriter writer)
    {
        Update.Serialize(ref writer);
    }
}
