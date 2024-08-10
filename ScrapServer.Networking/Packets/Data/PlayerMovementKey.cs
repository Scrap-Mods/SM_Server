﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public enum PlayerMovementKey
{
    NONE = 0,
    JUMP = 1 << 0,
    CRAWL = 1 << 1,
    HORIZONTAL = 1 << 2,
    SPRINT = 1 << 3,
    AIM = 1 << 4,
}