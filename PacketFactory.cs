using SMServer.Packets;
using System.Reflection;

namespace SMServer
{
    internal class PacketFactory
    {
        private readonly Dictionary<byte, Type> _packetTypesById = new Dictionary<byte, Type>();

        public PacketFactory()
        {
            // Register packet types here
            RegisterPacketType<Hello>();
            RegisterPacketType<ServerInfo>();
            RegisterPacketType<ClientAccepted>();
            RegisterPacketType<FileChecksums>();
            RegisterPacketType<ChecksumsAccepted>();
            RegisterPacketType<ChecksumDenied>();
            RegisterPacketType<Character>();
            RegisterPacketType<JoinConfirmation>();
        }

        public static byte GetPacketId<T>() where T : IPacket
        {
            var field = typeof(T).GetField("PacketId", BindingFlags.Public | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(byte))
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' does not declare a public static byte field 'PacketId'.");
            return (byte)field.GetValue(null);
        }

        public void RegisterPacketType<T>() where T : IPacket
        {
            _packetTypesById[GetPacketId<T>()] = typeof(T);
        }

        public Type? GetPacketById(byte Id)
        {
            _packetTypesById.TryGetValue(Id, out var type);
            return type;
        }

        public IPacket? ReadPacket(byte[] data)
        {

            using (var stream = new MemoryStream(data))
            using (var reader = new BigEndianBinaryReader(stream))
            {
                var id = reader.ReadByte();

                // Check if the packet type is registered
                var packetType = GetPacketById(id);
                if (packetType == null)
                    return null;

                var packet = (IPacket)Activator.CreateInstance(packetType)!;

                // Save reader position
                var position = reader.BaseStream.Position;
                byte[] decompressedData = LZ4.Decompress(reader.ReadBytes((int)(reader.BaseStream.Length - position)));
                
                using (var dStream = new MemoryStream(decompressedData))
                using (var dReader = new BigEndianBinaryReader(dStream))
                {
                    packet.Deserialize(dReader);
                }

                return packet;
            }
        }

        public byte[] WritePacket<T>(T packet) where T : IPacket
        {
            using (var stream = new MemoryStream())
            using (var writer = new BigEndianBinaryWriter(stream))
            {
                // Write packet id
                writer.Write(GetPacketId<T>());

                using (var cStream = new MemoryStream())
                using (var cWriter = new BigEndianBinaryWriter(cStream))
                {
                    packet.Serialize(cWriter);
                    byte[] compressedData = LZ4.Compress((cWriter.BaseStream as MemoryStream)!.ToArray());
                    writer.Write(compressedData);
                }

                return stream.ToArray();
            }
        }
    }
}
