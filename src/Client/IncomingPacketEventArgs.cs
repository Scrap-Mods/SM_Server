using SMServer.Packets;

namespace SMServer.src.Client
{
    public struct IncomingPacketEventArgs<T> where T : IPacket
    {
        public IClient Client { get; }
        public byte PacketId { get; }
        public T Packet { get; }

        public IncomingPacketEventArgs(IClient client, byte packetId, T packet)
        {
            Client = client;
            PacketId = packetId;
            Packet = packet;
        }
    }
}
