using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public struct NetworkInitialization : IBitSerializable
{
    public readonly void Serialize(ref BitWriter writer) {
    }

    public readonly void Deserialize(ref BitReader reader) {
    }
}
