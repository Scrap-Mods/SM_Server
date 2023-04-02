using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace SMServer
{
    internal class SMSocketManager : SocketManager
    {
        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            base.OnConnected(connection, info);
            connection.Accept();
        }

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            base.OnConnecting(connection, info);
            connection.SendMessage(new byte[] { 3 });
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
            Console.WriteLine(" Data: " + BitConverter.ToString(managedArray));
            Console.WriteLine(" Size: " + size);
        }
    }
}
