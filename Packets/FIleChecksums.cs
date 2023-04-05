using static SMServer.Packets.ServerInfo;

namespace SMServer.Packets
{
    [Serializable]
    internal class FileChecksums : IPacket
    {
        public const byte PacketId = 6;

        public UInt32[] Checksums;

        public FileChecksums()
        {

        }

        // Constructor
        public FileChecksums(uint[] checksums)
        {
            Checksums = checksums;
        }

        public virtual void Serialize(BigEndianBinaryWriter writer)
        {
            writer.Write((UInt32)Checksums.Length);
            foreach (var checksum in Checksums)
            {
                writer.Write(checksum);
            }
        }

        public virtual void Deserialize(BigEndianBinaryReader reader)
        {
            // Read the deserialized data from the decompressed stream
            UInt32 length = reader.ReadUInt32();
            Checksums = new UInt32[length];
            for (int i = 0; i < length; i++)
            {
                Checksums[i] = reader.ReadUInt32();
            }
        }
    }
}
