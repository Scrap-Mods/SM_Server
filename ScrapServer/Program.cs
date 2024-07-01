using Steamworks;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Client.Steam;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using ScrapServer.Networking.Client;

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

        var server = new SteamworksServer();

        server.ClientConnecting += (o, args) =>
        {
            Console.WriteLine($"Client connecting... {args.Client.Username}");
            args.Client.AcceptConnection();
        };

        server.ClientConnected += (o, args) =>
        {
            Console.WriteLine($"Client connected! {args.Client.Username}");
            args.Client.SendPacket(new ClientAccepted());
        };

        server.HandlePacket<Hello>((o, args) =>
        {
            Console.WriteLine("Received Hello");
            args.Client.SendPacket(new ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 397817921,
                GameTick = 0,
                ModData = new ModData[0],
                SomeData = new byte[0],
                GenericData = new BlobDataRef[0],
                ScriptData = new BlobDataRef[0],
                Flags = ServerFlags.None
            });
            Console.WriteLine("Sent ServerInfo");

            args.Client.SendPacket(new GenericDataS2C());
            Console.WriteLine("Sent Initialization Data");
        });

        server.HandlePacket<FileChecksums>((o, args) =>
        {
            Console.WriteLine("Received FileChecksum");
            args.Client.SendPacket(new ChecksumsAccepted());
            Console.WriteLine("Sent ChecksumAccepted");
        });

        server.HandlePacket<CharacterInfo>((o, args) =>
        {
            Console.WriteLine("Received CharacterInfo");
            args.Client.SendPacket(new JoinConfirmation());
            Console.WriteLine("Sent JoinConfirmation");
        });
        Console.WriteLine("Sent ClientAccepted");

        server.HandleRaw(PacketId.CompoundPacket, (o, args) =>
        {
            Console.WriteLine("Received a compound packet, ignoring...");
        });



        while (true)
        {
            server.Poll();

            // if user pressed DEL in console, close the server:
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Delete)
            {
                Console.WriteLine("exiting...");
                break;
            }
        }
        server.Dispose();
        SteamClient.Shutdown();
    }
}
