namespace SMServer.Packets
{
    internal class Checksums : IPacket
    {
        public static readonly byte Id = 6;

        // Constructor
        public Checksums()
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
            // Checksums packet has no additional data to deserialize
        }
    }
}
