﻿namespace ScrapServer.Networking.Packets.Data;

[Flags]
public enum ServerFlags : byte
{
    None = 0,
    DeveloperMode = 1
}