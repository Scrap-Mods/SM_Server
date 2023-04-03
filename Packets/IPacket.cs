using System.IO;
using System.Linq;
using System.Reflection;

namespace SMServer.Packets
{
    internal abstract class IPacket
    {
        public static readonly byte Id = 0;
        public string PacketName => GetType().Name;

        public abstract byte[] Serialize();

        public static IPacket? Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                var id = reader.ReadByte();
                var packetType = GetPacketTypeById(id);

                if (packetType == null)
                    return null;

                var packet = (IPacket?)Activator.CreateInstance(packetType);
                packet?.Deserialize(reader);
                return packet;
            }
        }

        protected abstract void Deserialize(BinaryReader reader);

        private static Type GetPacketTypeById(byte id)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(IPacket)))
                .FirstOrDefault(t => t.GetField("Id")?.GetValue(null).Equals(id) == true);
        }
    }
}
