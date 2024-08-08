using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;
using Steamworks;
using OpenTK.Mathematics;

namespace ScrapServer.Networking.Packets.Data;
public class CreateCharacter
{
    public UInt32 NetObjId;
    public SteamId SteamId;
    public Vector3 Position;
    public UInt16 WorldId;
    public float Yaw;
    public float Pitch;
    public Guid CharacterUUID;

    public void Deserialize(ref BitReader reader)
    {
        NetObjId = reader.ReadUInt32();
        SteamId = reader.ReadUInt64();
        Position = reader.ReadVector3ZYX();
        WorldId = reader.ReadUInt16();
        Yaw = reader.ReadSingle();
        Pitch = reader.ReadSingle();
        CharacterUUID = reader.ReadGuid();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(NetObjId);
        writer.WriteUInt64(SteamId.Value);
        writer.WriteVector3ZYX(Position);
        writer.WriteUInt16(WorldId);
        writer.WriteSingle(Yaw);
        writer.WriteSingle(Pitch);
        writer.WriteGuid(CharacterUUID);
    }
}