﻿namespace SMServer.Packets
{
    [Serializable]
    internal class Hello : IPacket
    {
        public const byte PacketId = 1;

        // Constructor
        public Hello()
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
