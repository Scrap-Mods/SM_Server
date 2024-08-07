using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapServer.Networking;

namespace ScrapServer.Core;

public struct Player
{
    public int CharacterID;
    public string Name;
}

public static class PlayerService
{
    public static Dictionary<IClient, Player> Players = new Dictionary<IClient, Player>();

    public static void Tick(UInt32 tick)
    {
    }
}
