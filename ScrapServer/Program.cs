using Steamworks;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;

namespace ScrapServer;

internal class Program
{
    static void Main(string[] args)
    {
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
        SteamFriends.SetRichPresence("passphrase", server_passphrase);
        SteamFriends.SetRichPresence("connect", string.Format("-connect_steam_id {0} -friend_steam_id {0}", steamid));

        var socketManager = SteamNetworkingSockets.CreateRelaySocket<SmartSocket>();

        socketManager.ReceivePacket<Hello>((conn, packet) => {
            socketManager.SendPacket(conn, new ServerInfo(
                723, // protocol ver
                ServerInfo.EGamemode.FlatTerrain,
                397817921, // seed
                0, // game tick
                new ServerInfo.ModData[0],
                new byte[0],
                new ServerInfo.GenericData[0],
                new ServerInfo.GenericData[0],
                0 // flags
            ));
        });

        socketManager.ReceivePacket<FileChecksums>((conn, packet) => {
            socketManager.SendPacket(conn, new ChecksumsAccepted());
        });

        socketManager.ReceivePacket<Character>((conn, packet) => {
            socketManager.SendPacket(conn, new JoinConfirmation());
        });

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
    }
}
