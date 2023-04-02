using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMServer.Packets
{
    internal class ClientAccepted : IPacket
    {
        // Constructor
        ClientAccepted(byte id=5) : base(id)
        {
        }

        public static ClientAccepted Deserialize(byte[] data)
        {
            return new ClientAccepted(data[0]);
        }

        public override byte[] Serialize()
        {
            return new byte[] { this.Id };
        }
    }
}
