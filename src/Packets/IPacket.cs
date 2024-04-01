using System.IO;
using System.Linq;
using System.Reflection;

namespace SMServer.Packets
{
    internal interface IPacket
    {
        public abstract static byte PacketId { get; }

        internal string PacketName => GetType().Name;

        public abstract void Serialize(BinaryWriter writer);

        public abstract void Deserialize(BinaryReader reader);
    }
}
