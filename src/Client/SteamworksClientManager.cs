using SMServer.Packets;
using Steamworks;
using Steamworks.Data;

namespace SMServer.src.Client
{
    public class SteamworksClientManager : IClientManager
    {
        private class SocketInterface : ISocketManager
        {
            private readonly SteamworksClientManager clientManager;

            public SocketInterface(SteamworksClientManager clientManager)
            {
                this.clientManager = clientManager;
            }

            public void OnConnecting(Connection connection, ConnectionInfo info)
            {
                var client = new SteamworksClient(clientManager, connection);
                var args = new ClientConnectingEventArgs(client);
                clientManager.ClientConnecting?.Invoke(clientManager, args);
                if (args.IsAccepted)
                {
                    connection.Accept();
                }
                else
                {
                    connection.Close();
                }
            }

            public void OnConnected(Connection connection, ConnectionInfo info)
            {
                var client = new SteamworksClient(clientManager, connection);
                clientManager.connectedClients.Add(client);
                client.State = ClientState.Connected;
                clientManager.ClientConnected?.Invoke(clientManager, new ClientEventArgs(client));
            }            

            public void OnDisconnected(Connection connection, ConnectionInfo info)
            {
                var connectingClients = clientManager.connectingClients;
                for (int i = 0; i < connectingClients.Count; i++)
                {
                    var client = connectingClients[i];
                    if (client.Connection == connection)
                    {
                        client.State = ClientState.Disconnected;
                        connectingClients.RemoveAt(i);
                        return;
                    }
                }
                var connectedClients = clientManager.connectedClients;
                for (int i = 0; i < connectedClients.Count; i++)
                {
                    var client = connectedClients[i];
                    if (client.Connection == connection)
                    {
                        client.State = ClientState.Disconnected;
                        connectedClients.RemoveAt(i);
                        return;
                    }
                }
            }

            public unsafe void OnMessage(Connection connection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel)
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<ClientConnectingEventArgs>? ClientConnecting;
        public event EventHandler<ClientEventArgs>? ClientConnected;
        public event EventHandler<ClientEventArgs>? ClientDisconnected;

        public IReadOnlyList<IClient> ConnectedClients => connectedClients;
        private readonly List<SteamworksClient> connectedClients;
        private readonly List<SteamworksClient> connectingClients;

        private readonly SocketManager socketManager;
        private readonly Queue<(Connection, IPacket)> outcomingPackets;

        public SteamworksClientManager()
        {
            connectedClients = new List<SteamworksClient>();
            connectingClients = new List<SteamworksClient>();

            outcomingPackets = new Queue<(Connection, IPacket)>();

            socketManager = new SocketManager
            {
                Interface = new SocketInterface(this)
            };
        }

        public void RunLoop(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                lock (outcomingPackets)
                {
                    while (outcomingPackets.TryDequeue(out var item))
                    {
                        var (connection, packet) = item;
                        using var cStream = new MemoryStream();
                        using var cWriter = new BigEndianBinaryWriter(cStream);
                        packet.Serialize(cWriter);
                        byte[] compressedData = LZ4.Compress((cWriter.BaseStream as MemoryStream)!.ToArray());
                        connection.SendMessage(compressedData);
                    }
                }
                socketManager.Receive();
            }
        }

        internal void EnqueueOutcomingPacket(Connection connection, IPacket packet)
        {
            lock (outcomingPackets)
            {
                outcomingPackets.Enqueue((connection, packet));
            }
        }

        internal void AddRegularCallback()
        {

        }

        internal void AddOneTimeCallback()
        {

        }

    }
}
