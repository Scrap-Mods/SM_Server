namespace SMServer.Packets
{
    [Serializable]
    internal class ChecksumsAccepted : IPacket
    {
        public static byte PacketId { get => 7; }

        // Constructor
        public ChecksumsAccepted()
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
