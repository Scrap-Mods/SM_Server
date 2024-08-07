using ScrapServer.Utility.Serialization;
using ScrapServer.Networking.Packets.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ScrapServer.Networking.Packets;

public struct PlayerMovement : IPacket
{
    /// <value>The packet id.</value>
    public static PacketId PacketId => PacketId.PlayerMovement;
    public static bool IsCompressable => true;

    public UInt32 Tick;
    public PlayerMovementKey Keys;
    public byte Direction;
    public byte Yaw;
    public byte Pitch;

    public void Deserialize(ref BitReader reader)
    {
        Tick = reader.ReadUInt32();
        Keys = (PlayerMovementKey) reader.ReadByte();
        Direction = reader.ReadByte();
        Yaw = reader.ReadByte();
        Pitch = reader.ReadByte();
    }

    public void Serialize(ref BitWriter writer)
    {
        throw new NotImplementedException();
    }
}
