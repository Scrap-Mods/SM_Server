namespace SMServer.Packets
{
    [Serializable]
    internal class JoinConfirmation : IPacket
    {
        public const byte PacketId = 10;

        // Constructor
        public JoinConfirmation()
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
