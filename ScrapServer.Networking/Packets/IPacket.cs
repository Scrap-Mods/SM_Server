﻿using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

/// <summary>
/// Represents a packet sent over the network.
/// </summary>
public interface IPacket : IBitSerializable
{
    /// <summary>
    /// Gets the unique identifier of the packet type.
    /// </summary>
    /// <value>The packet id.</value>
    public virtual static PacketId PacketId => PacketId.Empty;

    /// <summary>
    /// Gets whether the packets of this type should be compressed or not.
    /// </summary>
    /// <value><see langword="true"/> if the packets are compressable, <see langword="false"/> if not.</value>
    public virtual static bool IsCompressable => false;
}
