using ScrapServer.Utility.Serialization;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;
public struct CreateCharacter
{
    public UInt32 NetObjId;
    public SteamId SteamId;
    public Vector3f Position;
    public UInt16 WorldId;
    public float Yaw;
    public float Pitch;
    public Guid CharacterUUID;

    public void Deserialize(ref BitReader reader)
    {
        NetObjId = reader.ReadUInt32();
        SteamId = reader.ReadUInt64();
        Position.ReadXYZ(ref reader);
        WorldId = reader.ReadUInt16();
        Yaw = reader.ReadSingle();
        Pitch = reader.ReadSingle();
        CharacterUUID = reader.ReadGuid();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(NetObjId);
        writer.WriteUInt64(SteamId.Value);
        Position.WriteXYZ(ref writer);
        writer.WriteUInt16(WorldId);
        writer.WriteSingle(Yaw);
        writer.WriteSingle(Pitch);
        writer.WriteGuid(CharacterUUID);
    }
}