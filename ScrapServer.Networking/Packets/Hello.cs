﻿namespace ScrapServer.Networking.Packets;

public class Hello : IPacket
{
    public static byte PacketId { get => 1; }

    // Constructor
    public Hello()
    {
    }

    public void Serialize(BinaryWriter writer)
    {
    }

    public void Deserialize(BinaryReader reader)
    {
    }
}
