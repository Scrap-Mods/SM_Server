namespace SMServer.Packets
{
    [Serializable]
    internal class ChecksumsAccepted : IPacket
    {
        public const byte PacketId = 7;

        // Constructor
        public ChecksumsAccepted()
        {
        }

        public void Serialize(BigEndianBinaryWriter writer)
        {
        }

        public void Deserialize(BigEndianBinaryReader reader)
        {
        }
    }
}
