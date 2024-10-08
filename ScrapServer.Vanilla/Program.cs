﻿using Steamworks;
using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;
using ScrapServer.Core;
using System.Text;
using OpenTK.Mathematics;

namespace ScrapServer.Vanilla;

internal class Program
{

    static void Main(string[] args)
    {
        try
        {
            SteamClient.Init(387990);
            Console.WriteLine("Logged in as \"" + SteamClient.Name + "\" (" + SteamClient.SteamId + ")");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }

        string steamid = SteamClient.SteamId.ToString();
        const string server_passphrase = "balls";

        SteamFriends.SetRichPresence("status", "Hosting game");
        SteamFriends.SetRichPresence("passphrase", server_passphrase);
        SteamFriends.SetRichPresence("connect", string.Format("-connect_steam_id {0} -friend_steam_id {0}", steamid));

        PlayerService.Init();

        SchedulerService.Tick += PlayerService.Tick;
        SchedulerService.Tick += CharacterService.Tick;

        SchedulerService.Start();
    }
}
