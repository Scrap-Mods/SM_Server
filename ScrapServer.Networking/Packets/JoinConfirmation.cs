﻿namespace ScrapServer.Networking.Packets;

public class JoinConfirmation : IPacket
{
    public static byte PacketId { get => 10; }
    
    // Constructor
    public JoinConfirmation()
    {
    }

    public void Serialize(BinaryWriter writer)
    {
        
    }

    public void Deserialize(BinaryReader reader)
    {
    }
}
