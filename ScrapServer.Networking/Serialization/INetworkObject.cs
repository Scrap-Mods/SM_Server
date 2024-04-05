namespace ScrapServer.Networking.Serialization;

public interface INetworkObject
{
    public void Deserialize(ref PacketReader packetReader);

    public void Serialize(ref PacketWriter packetWriter);
}
