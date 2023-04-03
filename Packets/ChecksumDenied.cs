namespace SMServer.Packets
{
    internal class ChecksumDenied : IPacket
    {
        public static readonly byte Id = 8;

        UInt32 Index;

        // Constructor
        public ChecksumDenied(UInt32 index)
        {
            this.Index = index;
        }

        public override byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BigEndianBinaryWriter(stream))
            {
                writer.Write(Id);
                writer.Write(Index);

                return stream.ToArray();
            }
        }

        protected override void Deserialize(BinaryReader reader)
        {

        }
    }
}
