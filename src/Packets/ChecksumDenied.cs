namespace SMServer.Packets
{
    [Serializable]
    internal class ChecksumDenied : IPacket
    {
        public static byte PacketId { get => 8; }

        public UInt32 Index;

        public ChecksumDenied()
        {

        }

        // Constructor
        public ChecksumDenied(UInt32 index)
        {
            this.Index = index;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Index);
        }

        public void Deserialize(BinaryReader reader)
        {
        }
    }
}
