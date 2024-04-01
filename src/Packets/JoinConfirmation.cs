namespace SMServer.Packets
{
    [Serializable]
    internal class JoinConfirmation : IPacket
    {
        public static byte PacketId { get => 10; }
        
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
