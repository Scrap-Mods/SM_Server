using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using SMServer.Packets;
using Steamworks;
using Steamworks.Data;

namespace SMServer
{
    internal class SmartSocket : SocketManager
    {
        private readonly Dictionary<byte, Type> id2packet = new Dictionary<byte, Type>();
        private readonly Dictionary<byte, List<Delegate>> callbacks = new Dictionary<byte, List<Delegate>>();
                
        public SmartSocket()
        {
        }

        private static byte GetPacketId<T>() where T : IPacket
        {
            var field = typeof(T).GetField("PacketId", BindingFlags.Public | BindingFlags.Static);
            if (field == null || field.FieldType != typeof(byte))
                throw new InvalidOperationException($"Type '{typeof(T).FullName}' does not declare a public static byte field 'PacketId'.");
            return (byte)field.GetValue(null);
        }

        public void SendPacket<T>(Connection connection, T packet) where T : IPacket
        {
            using (var cStream = new MemoryStream())
            using (var cWriter = new BigEndianBinaryWriter(cStream))
            {
                packet.Serialize(cWriter);
                byte[] compressedData = LZ4.Compress((cWriter.BaseStream as MemoryStream)!.ToArray());
            
                connection.SendMessage(compressedData, SendType.NoNagle);
            }
        }

        public void ReceivePacket<T>(Action<Connection, NetIdentity, T> func) where T : IPacket
        {
            if (!id2packet.ContainsKey(GetPacketId<T>())) id2packet[GetPacketId<T>()] = typeof(T);
            if (!callbacks.ContainsKey(GetPacketId<T>())) callbacks[GetPacketId<T>()] = new List<Delegate>();

            callbacks[GetPacketId<T>()].Add(func);
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
            connection.Accept();
        }

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            base.OnConnecting(connection, info);
            connection.SendMessage(new byte[] { 5 });
        }

        public override void OnConnectionChanged(Connection connection, ConnectionInfo info)
        {
            base.OnConnectionChanged(connection, info);
            Console.WriteLine("--------------------------------------------\n Connection Status Changed");
            Console.WriteLine(" Connection handle: " + connection + ", user: " + info.Identity.ToString());
            Console.WriteLine(" State: " + "Unknown" + " -> " + info.State.ToString());
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            base.OnDisconnected(connection, info);
            Console.WriteLine("--------------------------------------------\n OnDisconnected");
            Console.WriteLine(" Connection handle: " + connection + ", user: " + info.Identity.ToString());
            Console.WriteLine(" State: " + "Unknown" + " -> " + info.State.ToString());
        }

        public override void OnMessage(Connection connection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            byte[] managedArray = new byte[size];
            Marshal.Copy(data, managedArray, 0, size);

            Console.WriteLine("--------------------------------------------\n OnMessage");
            Console.WriteLine(" Connection handle: " + connection + ", user: " + identity.ToString());
            Console.WriteLine(" PacketID: " + managedArray[0]);
            Console.WriteLine(" Size: " + size);

            using (var stream = new MemoryStream(managedArray))
            using (var reader = new BigEndianBinaryReader(stream))
            {
                var id = reader.ReadByte();

                // Check if the packet type is registered
                id2packet.TryGetValue(id, out var packetType);
                if (packetType == null)
                {
                    Console.WriteLine("Unregistered packet!");
                    return;
                }

                //Must be dynamic since
                //A. We don't know what packetType is ahead of time.
                //B. We need to cast to the correct type before passing it to the callback
                dynamic packet = Activator.CreateInstance(packetType);

                // Save reader position
                var position = reader.BaseStream.Position;
                byte[] decompressedData = LZ4.Decompress(reader.ReadBytes((int)(reader.BaseStream.Length - position)));
                
                using (var dStream = new MemoryStream(decompressedData))
                using (var dReader = new BigEndianBinaryReader(dStream))
                {
                    packet.Deserialize(dReader);
                }

                Type[] typeArgs = { typeof(Connection), typeof(NetIdentity), packetType };
                var callbackType = typeof(Action<,,>).MakeGenericType(typeArgs);

                foreach (var callback in callbacks[id])
                {
                    //Also needs to be dynamic because we don't know what callback type we'll end up with.
                    dynamic cb = Convert.ChangeType(callback, callbackType);
                    cb(connection, identity, packet);
                }
            };

        }
    }
}
