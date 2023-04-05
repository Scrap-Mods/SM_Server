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

        public virtual void Serialize(BigEndianBinaryWriter writer)
        {
        }

        public virtual void Deserialize(BigEndianBinaryReader reader)
        {
        }
    }
}
