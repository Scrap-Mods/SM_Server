using Steamworks;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using System.Text;

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

        var server = new SteamworksServer();

        server.ClientConnecting += (o, args) =>
        {
            Console.WriteLine($"Client connecting... {args.Client.Username}");
            args.Client.AcceptConnection();
        };

        server.ClientConnected += (o, args) =>
        {
            Console.WriteLine($"Client connected! {args.Client.Username}");

            args.Client.Send(new ClientAccepted {});
            Console.WriteLine("Sent ClientAccepted");
        };

        server.Handle<FileChecksums>((o, args2) =>
        {
            Console.WriteLine("Received FileChecksum");

            // handled checksum?

            args2.Client.Send(new ChecksumsAccepted {});
            Console.WriteLine("Sent ChecksumAccepted");
        });

        server.Handle<CharacterInfo>((o, args2) =>
        {
            Console.WriteLine("Received CharacterInfo");

            // handle CharacterInfo

            args2.Client.Send(new JoinConfirmation {});
            Console.WriteLine("Sent JoinConfirmation");
        });

        server.Handle<CompoundPacket>((o, args2) => { });

        server.Handle<Hello>((o, args2) =>
        {
            Console.WriteLine("Received Hello");
            args2.Client.Send(new ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 397817921,
                GameTick = 0,
                Flags = ServerFlags.None
            });
            Console.WriteLine("Sent ServerInfo");

            var characterCustomization = new CharacterCustomization
            {
                Gender = Gender.Male,
                Items = [
                    new CharacterItem
                    {
                        PaletteIndex = 1,
                        VariantId = Guid.Parse("cc273e37-4ba8-498e-ac5e-3497403c7fe2")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("87b7d156-8b83-4612-9cb1-93b768de8dc1")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("00000000-0000-0000-0000-000000000000")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("915a8a36-3bbc-427e-ae00-4954208053b9")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("8de62bd5-c7b3-4b2c-8d30-f986dff5ef70")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("18fdfb7c-42ae-463c-8ee5-d2ce271c3ec8")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("803b41c1-a6f9-475c-953d-dd78c5ae99c4")
                    },
                    new CharacterItem
                    {
                        PaletteIndex = 0,
                        VariantId = Guid.Parse("e5ba292b-1772-463f-8010-b97c48bc9298")
                    },
                ]
            };

            var playerData = new PlayerData
            {
                CharacterID = 1,
                SteamID = 76561198142527219,
                InventoryContainerID = 3,
                CarryContainer = 2,
                CarryColor = uint.MaxValue,
                Name = "TechnologicNickFR",
                CharacterCustomization = characterCustomization,
            };

            var initBlobData = new BlobData
            {
                Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                Key = [0x2, 0x00, 0x00, 0x00],
                WorldID = 65534,
                Flags = 13,
                Data = playerData.ToBytes()
            };

            var genericInit = new GenericInitData { Data = [initBlobData], GameTick = 1 };
            args2.Client.Send(genericInit);
            Console.WriteLine("Sent Initialization Data");

            BlobData[] scriptBlobData = [
                new BlobData {
                    Uid = Guid.Parse("4a293a1d-b223-520a-a3ac-0f9a7ded3869"),
                    Key = [0x2, 0, 0, 0],
                    WorldID = 65534,
                    Flags = 4,
                    Data = Encoding.ASCII.GetBytes("\x07LUA\x00\x00\x00\x01\x05\x00\x00\x00\x02\x02\x00\x00\x00\x02\x80inOil\x02\x02\x00\x00\x00\x05\x00inChemical\x02\x00"),
                },
                new BlobData {
                    Uid = Guid.Parse("4a293a1d-b223-520a-a3ac-0f9a7ded3869"),
                    Key = [0x1, 0, 0, 0],
                    WorldID = 65534,
                    Flags = 4,
                    Data = Encoding.ASCII.GetBytes("\x07LUA\x00\x00\x00\x01\x05\x00\x00\x00\x02\x02\x00\x00\x00\x02\x80inOil\x02\x02\x00\x00\x00\x05\x00inChemical\x02\x00"),
                },
                new BlobData {
                    Uid = Guid.Parse("20896033-23a4-5789-a03c-a7533e3bff84"),
                    Key = [0x1, 0, 0, 0],
                    WorldID = 65534,
                    Flags = 4,
                    Data = Encoding.ASCII.GetBytes("\x04LUA\x00\x00\x00\x01\x05\x00\x00\x00\x01\x02\x00\x00\x00\x02\x00time\x03?\x00\x00\x00"),
                },
            ];

            var compound = new CompoundPacket.Builder()
                .Write(new InitNetworkUpdate { GameTick = 2, Data = [
                .Write(new ScriptDataS2C { GameTick = 0, Data = scriptBlobData })
                .Build();

            args2.Client.Send(compound);
            Console.WriteLine("Sent InitializationNetworkUpdate");
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
