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
                CharacterID = 0xFFFFFFFF,
                SteamID = args2.Client.Id,
                InventoryContainerID = 3,
                CarryContainer = 4,
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

            var genericInit = new GenericInitData { Data = [initBlobData], GameTick = 3 };
            args2.Client.Send(genericInit);


            playerData = new PlayerData
            {
                CharacterID = 2,
                SteamID = args2.Client.Id,
                InventoryContainerID = 3,
                CarryContainer = 4,
                CarryColor = uint.MaxValue,
                Name = "TechnologicNickFR",
                CharacterCustomization = characterCustomization,
            };

            initBlobData = new BlobData
            {
                Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                Key = [0x2, 0x00, 0x00, 0x00],
                WorldID = 65534,
                Flags = 13,
                Data = playerData.ToBytes()
            };

            genericInit = new GenericInitData { Data = [initBlobData], GameTick = 3 };
            args2.Client.Send(genericInit);

            Console.WriteLine("Sent Client GenericInitData");

            BlobData[] scriptBlobData = [
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

            var stream = BitWriter.WithSharedPool();
            var netObj = new NetObj { ObjectType = NetObjType.Character, UpdateType = NetworkUpdateType.Create, Size = 0 };
            var createUpdate = new CreateUpdate { ControllerType = (ControllerType)0 };
            var characterCreate = new CreateCharacter { NetObjId = 2, SteamId = args2.Client.Id, Position = new Vector3f { X = 0, Y = 0, Z = 1 }, CharacterUUID = Guid.Empty, Pitch = 0, Yaw = 0, WorldId = 1 };

            netObj.Serialize(ref stream);
            createUpdate.Serialize(ref stream);
            characterCreate.Serialize(ref stream);
            NetObj.WriteSize(ref stream, 0);

            var data = stream.Data.ToArray();
            stream.Dispose();

            var compound = new CompoundPacket.Builder()
                .Write(new InitNetworkUpdate { GameTick = 4, Updates = data })
                .Write(new ScriptDataS2C { GameTick = 4, Data = scriptBlobData })
                .Build();

            Console.WriteLine("Sent ScriptInitData and NetworkInitUpdate for Client");
        });

        server.Handle<CompoundPacket>((o, args2) => { });

        server.Handle<Hello>((o, args2) =>
        {
            Console.WriteLine("Received Hello");
            args2.Client.Send(new ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 1023853875,
                GameTick = 0,
                GenericData = [
                    new BlobDataRef {
                        Uid =  Guid.Parse("44ac020c-aec7-4f8b-b230-34d2e3bd23eb"),
                        Key = [0x00, 0x00, 0x00, 0x00],
                    },
                    new BlobDataRef {
                        Uid =  Guid.Parse("3ff36c8b-93f7-4428-ae4d-429a6f0cf77d"),
                        Key = [0x01, 0x00, 0x00, 0x00],
                    },
                    new BlobDataRef {
                        Uid =  Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                        Key = [0x01, 0x00, 0x00, 0x00],
                    },
                ],
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
                SteamID = 76561198158782028,
                InventoryContainerID = 3,
                CarryContainer = 2,
                CarryColor = uint.MaxValue,
                Name = "Prime",
                CharacterCustomization = characterCustomization,
            };

            var initBlobData = new BlobData
            {
                Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                Key = [0x1, 0x00, 0x00, 0x00],
                WorldID = 65534,
                Flags = 13,
                Data = playerData.ToBytes()
            };

            var gameplayOptions = new BlobData
            {
                Uid = Guid.Parse("44ac020c-aec7-4f8b-b230-34d2e3bd23eb"),
                Key = [0x0, 0x00, 0x00, 0x00],
                WorldID = 65534,
                Flags = 13,
                Data = Encoding.ASCII.GetBytes("\x00\x34{\"Difficulty\":1,\"Multiplayer\":3,\"PhysicsQuality\":8}\n"),
            };

            var worldData = new WorldData { TerrainParams = "{\"worldFile\":\"\"}\n", Classname = "CreativeFlatWorld", Seed = 1023853875, Filename = "$GAME_DATA/Scripts/game/worlds/CreativeFlatWorld.lua" };

            var worldOptions = new BlobData
            {
                Uid = Guid.Parse("3ff36c8b-93f7-4428-ae4d-429a6f0cf77d"),
                Key = [0x1, 0x00, 0x00, 0x00],
                WorldID = 1,
                Flags = 13,
                Data = worldData.ToBytes(),
            };

            var genericInit = new GenericInitData { Data = [gameplayOptions, worldOptions, initBlobData], GameTick = 1 };
            args2.Client.Send(genericInit);

            Console.WriteLine("Sent Host Character Generic Initialization Data");
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
