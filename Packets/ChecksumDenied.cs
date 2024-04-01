﻿namespace SMServer.Packets
{
    [Serializable]
    internal class ChecksumDenied : IPacket
    {
        public const byte PacketId = 8;

        public UInt32 Index;

        public ChecksumDenied()
        {

        }

        // Constructor
        public ChecksumDenied(UInt32 index)
        {
            this.Index = index;
        }

        public void Serialize(BigEndianBinaryWriter writer)
        {
            writer.Write(Index);
        }

        public void Deserialize(BigEndianBinaryReader reader)
        {
        }
    }
}
