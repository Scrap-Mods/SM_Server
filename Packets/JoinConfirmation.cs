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

        public void Serialize(BigEndianBinaryWriter writer)
        {
        }

        public void Deserialize(BigEndianBinaryReader reader)
        {
        }
    }
}
