using Microsoft.VisualBasic;
using SMServer;
using SMServer.Packets;
using Steamworks;

try
{
    SteamClient.Init(387990);
    Console.WriteLine("Logged in as \"" + SteamClient.Name + "\" (" + SteamClient.SteamId + ")");
}
catch (System.Exception e)
{
    // Couldn't init for some reason (steam is closed etc)
    Console.WriteLine(e.Message);
}

string steamid = SteamClient.SteamId.ToString();
const string server_passphrase = "balls";

SteamFriends.SetRichPresence("status", "Hosting game");
SteamFriends.SetRichPresence("Passphrase", server_passphrase);
SteamFriends.SetRichPresence("connect", string.Format("-connect_steam_id {0} -friend_steam_id {0}", steamid));

var socketManager = SteamNetworkingSockets.CreateRelaySocket<SMSocketManager>();

while (true)
{
    socketManager.Receive();

    // if user pressed DEL in console, close the server:
    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Delete)
    {
        Console.WriteLine("exiting...");
        break;
    }
}

socketManager.Close();
SteamClient.Shutdown();