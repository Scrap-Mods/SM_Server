﻿using Steamworks;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using ScrapServer.Core;
using System.Text;
using OpenTK.Mathematics;
using System.Numerics;

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
        UInt32 tick = 0;

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

            args2.Client.Send(new JoinConfirmation { });
            Console.WriteLine("Sent JoinConfirmation");

            var player = PlayerService.GetPlayer(args2.Client);
            var character = CharacterService.GetCharacter(player);

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
                        VariantId = Guid.Empty
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
                CharacterID = character.Id,
                SteamID = args2.Client.Id,
                InventoryContainerID = 2,
                CarryContainer = 4,
                CarryColor = uint.MaxValue,
                PlayerID = (byte)player.Id,
                Name = "TechnologicNickFR",
                CharacterCustomization = characterCustomization,
            };

            var initBlobData = new BlobData
            {
                Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                Key = BitConverter.GetBytes(character.Id),
                WorldID = 65534,
                Flags = 13,
                Data = playerData.ToBytes()
            };

            var genericInit = new GenericInitData { Data = [initBlobData], GameTick = tick };

            foreach (var client in PlayerService.Players.Keys)
            {
                client.Send(genericInit);
            }

            Console.WriteLine("Sent Client GenericInitData");

            var stream = BitWriter.WithSharedPool();
            var netObj = new NetObj { ObjectType = NetObjType.Character, UpdateType = NetworkUpdateType.Create, Size = 0 };
            var createUpdate = new CreateNetObj { ControllerType = (ControllerType)0 };
            var characterCreate = new CreateCharacter { NetObjId = (uint) character.Id, SteamId = args2.Client.Id, Position = new Vector3(0,0,0.72f), CharacterUUID = Guid.Empty, Pitch = 0, Yaw = 0, WorldId = 1 };

            netObj.Serialize(ref stream);
            createUpdate.Serialize(ref stream);
            characterCreate.Serialize(ref stream);
            NetObj.WriteSize(ref stream, 0);

            var data = stream.Data.ToArray();
            stream.Dispose();

            // Compound packet
            var compound = new CompoundPacket.Builder()
                .Write(new InitNetworkUpdate { GameTick = tick, Updates = data })
                .Write(new ScriptDataS2C { GameTick = tick, Data = [] })
                .Build();

            args2.Client.Send(compound);

            foreach (var client in PlayerService.Players.Keys)
            {
                if (client != args2.Client)
                {
                    client.Send(new NetworkUpdate { GameTick = tick, Updates = data });
                }
            }

            foreach (var client in PlayerService.Players)
            {
                if (client.Value == player) continue;

                stream = BitWriter.WithSharedPool();

                // Create packet
                character = CharacterService.GetCharacter(client.Value);
                netObj = new NetObj { ObjectType = NetObjType.Character, UpdateType = NetworkUpdateType.Create, Size = 0 };
                createUpdate = new CreateNetObj { ControllerType = (ControllerType)0 };
                characterCreate = new CreateCharacter { NetObjId = (uint)character.Id, SteamId = args2.Client.Id, Position = new Vector3(0, 0, 0.72f), CharacterUUID = Guid.Empty, Pitch = 0, Yaw = 0, WorldId = 1 };

                netObj.Serialize(ref stream);
                createUpdate.Serialize(ref stream);
                characterCreate.Serialize(ref stream);
                NetObj.WriteSize(ref stream, 0);

                // Update packet
                netObj = new NetObj { ObjectType = NetObjType.Character, UpdateType = NetworkUpdateType.Update, Size = 0 };
                var characterUpdate = new UpdateCharacter
                {
                    Color = new Networking.Packets.Data.Color4 { Alpha = 0xFF, Blue = 0xFF, Green = 0xFF, Red = 0xFF },
                    Movement = new MovementState { IsClimbing = false, IsDiving = false, IsDowned = false, IsSwimming = false, IsTumbling = false, Unknown = false },
                    SelectedItem = new Item { InstanceId = -1, Uuid = Guid.Empty }
                };

                stream.GoToNearestByte();
                var position = stream.ByteIndex;

                netObj.Serialize(ref stream);
                characterUpdate.Serialize(ref stream);
                NetObj.WriteSize(ref stream, position);

                data = stream.Data.ToArray();

                args2.Client.Send(new NetworkUpdate { GameTick = tick, Updates = data });

                stream.Dispose();
            }


            Console.WriteLine("Sent ScriptInitData and NetworkInitUpdate for Client");
        });

        server.Handle<PlayerMovement>((o, args2) =>
        {
            var player = PlayerService.GetPlayer(args2.Client);

            Console.WriteLine("Player: {0}, {1} Character: {2}", args2.Client.Id.ToString("D"), player.Id.ToString("D"), CharacterService.GetCharacter(player).Id);

            CharacterService.MoveCharacter(player, args2.Packet.Direction, args2.Packet.Keys);
            CharacterService.LookCharacter(player, args2.Packet.Yaw, args2.Packet.Pitch);
        });

        server.Handle<Hello>((o, args2) =>
        {
            Console.WriteLine("Received Hello");
            args2.Client.Send(new ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 1023853875,
                GameTick = tick,
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
                SomeData = ASCIIEncoding.UTF8.GetBytes("{}"),
                Flags = ServerFlags.DeveloperMode
            });
            Console.WriteLine("Sent ServerInfo");

            foreach (var character in PlayerService.Players)
            {
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
                        VariantId = Guid.Empty
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
                    SteamID = 0,
                    InventoryContainerID = 1,
                    CarryContainer = 2,
                    CarryColor = uint.MaxValue,
                    PlayerID = 0,
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
            }

            var gameplayOptions = new BlobData
            {
                Uid = Guid.Parse("44ac020c-aec7-4f8b-b230-34d2e3bd23eb"),
                Key = [0x0, 0x00, 0x00, 0x00],
                WorldID = 65534,
                Flags = 15,
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

            var genericInit = new GenericInitData { Data = [gameplayOptions, worldOptions, initBlobData], GameTick = tick };
            args2.Client.Send(genericInit);

            Console.WriteLine("Sent Host Character Generic Initialization Data");
        });

        server.Handle<Broadcast>((o, args2) =>
        {
            foreach (var client in server.ConnectedClients)
            {
                if (client != args2.Client)
                {
                    client.Send(args2.Packet);
                }
            }
            Console.WriteLine("BBL DRIZZY DRAKE");
        });

        while (true)
        {
            server.Poll();

            CharacterService.Tick(tick);
            PlayerService.Tick(tick);

            // if user pressed DEL in console, close the server:
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Delete)
            {
                Console.WriteLine("exiting...");
                break;
            }

            tick++;
            Thread.Sleep(25);
        }
        server.Dispose();
        SteamClient.Shutdown();
    }
}
