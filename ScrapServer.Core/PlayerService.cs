using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapServer.Networking;

namespace ScrapServer.Core;

public class Player
{
    public string Name = "MECHANIC";
    public int Id;
}

public static class PlayerService
{
    public static Dictionary<IClient, Player> Players = new Dictionary<IClient, Player>();
    public static int NextPlayerID = 1;

    public static Player GetPlayer(IClient client)
    {
        Player? player;
        var found = Players.TryGetValue(client, out player);

        if (found) return player;

        // If character isn't in Dict, we load it from the save database into the dict and return it

        // ...

        // If it still cannot be found, we make a new one
        player = new Player
        {
            Name = "TechnologicNickFR",
            Id = NextPlayerID,
        };

        Players[client] = player;
        NextPlayerID += 1;

        return player;
    }

    public static void Tick(UInt32 tick)
    {
    }
}
