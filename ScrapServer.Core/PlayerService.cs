﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScrapServer.Networking;

namespace ScrapServer.Core;

public class Player
{
    public int Id;
    public ulong SteamId;
    public string Name;
}

public static class PlayerService
{
    public static Dictionary<IClient, Player> Players = [];
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
            Id = NextPlayerID,
            SteamId = client.Id,
            Name = client.Username ?? "MECHANIC"
        };

        Players[client] = player;
        NextPlayerID += 1;

        return player;
    }

    public static void RemovePlayer(IClient client)
    {
        Player? player;
        var found = Players.TryGetValue(client, out player);

        if (player is Player ply)
        {
            CharacterService.RemoveCharacter(ply);
            Players.Remove(client);
        }

    }

    public static void Tick(UInt32 tick)
    {
    }
}
