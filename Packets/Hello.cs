namespace SMServer.Packets
{
    internal class Hello : IPacket
    {
        public static readonly byte Id = 1;

        // Constructor
        public Hello()
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
            // Hello packet has no additional data to deserialize
        }
    }
}
