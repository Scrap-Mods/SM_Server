namespace SMServer.Packets
{
    internal class ChecksumAccepted : IPacket
    {
        public static readonly byte Id = 7;

        // Constructor
        public ChecksumAccepted()
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
            // ChecksumAccepted packet has no additional data to deserialize
        }
    }
}
