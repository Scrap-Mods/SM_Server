using System.IO;

namespace SMServer.Packets
{
    [Serializable]
    internal class ClientAccepted : IPacket
    {
        public static byte PacketId { get => 5; }

        // Constructor
        public ClientAccepted()
        {
        }

        public void Serialize(BinaryWriter writer)
        {
        }

        public void Deserialize(BinaryReader reader)
        {
        }
    }
}
