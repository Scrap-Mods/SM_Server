using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Client
{
    public enum ClientState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2
    }
}
