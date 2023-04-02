using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMServer.Packets
{
    internal abstract class IPacket
    {
        protected byte Id;
        protected string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        protected IPacket(byte id)
        {
            Id = id;
        }

        public abstract byte[] Serialize();
    }
}