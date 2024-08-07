using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public enum PlayerMovementKey
{
    JUMP = 0,
    CRAWL = 1,
    HORIZONTAL = 2,
    SPRINT = 4,
    AIM = 8,
}