using Steamworks;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Steam;
using ScrapServer.Utility.Serialization;
using System.Text;

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

            args.Client.Send(PacketId.ClientAccepted, new NullPacket());
            Console.WriteLine("Sent ClientAccepted");
        };

        server.Handle(PacketId.Hello, (o, args) =>
        {
            Console.WriteLine("Received Hello");

            args.Client.Send(PacketId.ServerInfo, new ServerInfo
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

            args.Client.Send(PacketId.GenericDataS2C, new GenericDataS2C());
            Console.WriteLine("Sent Initialization Data");

            var data = Encoding.ASCII.GetBytes("\x00\x00\x00\xCE\x19\x00\x0b\x13xJ):\x1d\xb2#R\n\xa3\xac\x0f\x9a}\xed8i\x00\x04\x02\x00\x00\x00\xff\xfe\x04\x00\x00\x00+\xf1\x0e\x07LUA\x00\x00\x00\x01\x05\x00\x00\x00\x02\x02\x00\x00\x00\x05\x00inChemical\x11\x00\x90\x02\x80inOil\x02\x00J):\x1d\xb2#R\n\xa3\xac\x0f\x9a}\xed8i\x00\x04\x01\x00\x00\x00\xff\xfe\x04\x00\x00\x00+\xf1\x0e\x07LUA\x00\x00\x00\x01\x05\x00\x00\x00\x02\x02\x00\x00\x00\x05\x00inChemical\x11\x00\x90\x02\x80inOil\x02\x00 \x89`3#\xa4W\x89\xa0<\xa7S>;\xff\x84\x00\x04\x01\x00\x00\x00\xff\xfe\x04\x00\x00\x00\x1e\xf0\r\x04LUA\x00\x00\x00\x01\x05\x00\x00\x00\x01\x02\x00\x00\x00\x02\x00time\x03?\x00\x00\x00");
            args.Client.Send(PacketId.CompoundPacket, new RawPacket { Data = data });

            Console.WriteLine("Sent Raw Packet");
        });

        server.Handle(PacketId.FileChecksums, (o, args) =>
        {
            Console.WriteLine("Received FileChecksum");
            var checksum = args.Deserialize<FileChecksums>();

            // Validate checksum?

            args.Client.Send(PacketId.ChecksumsAccepted, new NullPacket());
            Console.WriteLine("Sent ChecksumAccepted");
        });

        server.Handle(PacketId.CharacterInfo, (o, args) =>
        {
            Console.WriteLine("Received CharacterInfo");
            var character = args.Deserialize<CharacterInfo>();

            // do stuff with the character

            args.Client.Send(PacketId.JoinConfirmation, new NullPacket());
            Console.WriteLine("Sent JoinConfirmation");
        });

        server.Handle(PacketId.CompoundPacket, (o, args) =>
        {
            Console.WriteLine("Received a compound packet");
            var reader = BitReader.WithSharedPool(args.Packet);

            // handle compound packet
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
