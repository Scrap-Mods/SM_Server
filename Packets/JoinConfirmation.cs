namespace SMServer.Packets
{
    internal class JoinConfirmation : IPacket
    {
        public static readonly byte Id = 10;

        // Constructor
        public JoinConfirmation()
        {
        }

        public override byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Id);
                return stream.ToArray();
            }
        }

        protected override void Deserialize(BinaryReader reader)
        {
            // packet has no additional data to deserialize
        }
    }
}
