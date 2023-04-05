using System.IO;

namespace SMServer.Packets
{
    [Serializable]
    internal class ClientAccepted : IPacket
    {
        public const byte PacketId = 5;

        // Constructor
        public ClientAccepted()
        {
        }

        public virtual void Serialize(BigEndianBinaryWriter writer)
        {
        }

        public virtual void Deserialize(BigEndianBinaryReader reader)
        {
        }
    }
}
