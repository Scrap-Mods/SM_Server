namespace SMServer.Packets
{
    [Serializable]
    internal class Hello : IPacket
    {
        public const byte PacketId = 1;

        // Constructor
        public Hello()
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
