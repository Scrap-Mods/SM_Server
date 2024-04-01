﻿using System.IO;
using System.Linq;
using System.Reflection;

namespace SMServer.Packets
{
    public interface IPacket
    {
        public virtual static byte PacketId => 0;

        internal string PacketName => GetType().Name;

        public abstract void Serialize(BinaryWriter writer);

        public abstract void Deserialize(BinaryReader reader);
    }
}