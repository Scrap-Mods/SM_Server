using System.IO;
using System.Linq;
using System.Reflection;

namespace SMServer.Packets
{
    internal interface IPacket
    {
        public const byte PacketId = 0;

        internal string PacketName => GetType().Name;

        public abstract void Serialize(BigEndianBinaryWriter writer);

        public abstract void Deserialize(BigEndianBinaryReader reader);
    }
}
