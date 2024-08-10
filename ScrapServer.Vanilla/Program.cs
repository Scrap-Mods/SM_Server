using Steamworks;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using ScrapServer.Core;
using System.Text;
using OpenTK.Mathematics;
using System.Numerics;
using System.ComponentModel.DataAnnotations;
using static System.Reflection.Metadata.BlobBuilder;

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

        server.ClientDisconnected += (o, args) =>
        {
            Console.WriteLine($"Client disconnected... {args.Client.Username}");
            PlayerService.RemovePlayer(args.Client);
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

            character.Name = args2.Packet.Name ?? "MECHANIC";
            character.Customization = args2.Packet.Customization;


            // Send Initialization Network Update
            List<byte> bytes = [];
            foreach (var ply in PlayerService.Players)
            {
                var Player = ply.Value;
                var Character = CharacterService.GetCharacter(Player);

                bytes.AddRange(Character.InitNetworkPacket(tick));
            }

            //var compound = new CompoundPacket.Builder();
            //compound.Write(new InitNetworkUpdate { GameTick = tick, Updates = bytes.ToArray() });
            //compound.Write(new ScriptDataS2C { GameTick = tick, Data = [] });
            //args2.Client.Send(compound.Build());

            args2.Client.Send(new InitNetworkUpdate { GameTick = tick, Updates = bytes.ToArray() });
            args2.Client.Send(new ScriptDataS2C { GameTick = tick, Data = [] });

            var newCharacterBlob = new GenericInitData { Data = [character.BlobData(tick)], GameTick = tick };

            foreach (var client in PlayerService.Players.Keys)
            {
                client.Send(newCharacterBlob);
                character.SpawnPackets(client, tick);
            }

            Console.WriteLine("Sent ScriptInitData and NetworkInitUpdate for Client");
        });

        server.Handle<PlayerMovement>((o, args2) =>
        {
            var player = PlayerService.GetPlayer(args2.Client);
            var Character = CharacterService.GetCharacter(player);

            Console.WriteLine("Player: {0}, {1} Character: {2}", args2.Client.Id.ToString("D"), player.Id.ToString("D"), CharacterService.GetCharacter(player).Id);

            Character.HandleMovement(args2.Packet);
        });

        server.Handle<Hello>((o, args2) =>
        {
            List<BlobDataRef> blobDataRefs = [
                new BlobDataRef {
                    Uid =  Guid.Parse("44ac020c-aec7-4f8b-b230-34d2e3bd23eb"),
                    Key = [0x00, 0x00, 0x00, 0x00],
                },
                new BlobDataRef {
                    Uid =  Guid.Parse("3ff36c8b-93f7-4428-ae4d-429a6f0cf77d"),
                    Key = [0x01, 0x00, 0x00, 0x00],
                },
            ];

            foreach (var ply in PlayerService.Players)
            {
                var Player = ply.Value;
                blobDataRefs.Add(new BlobDataRef {
                    Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                    Key = BitConverter.GetBytes(Player.Id),
                });
            }

            args2.Client.Send(new ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 1023853875,
                GameTick = tick,
                GenericData = blobDataRefs.ToArray(),
                SomeData = ASCIIEncoding.UTF8.GetBytes("{}"),
                Flags = ServerFlags.DeveloperMode
            });


            List<BlobData> blobs = [
                new BlobData
                {
                    Uid = Guid.Parse("44ac020c-aec7-4f8b-b230-34d2e3bd23eb"),
                    Key = [0x0, 0x00, 0x00, 0x00],
                    WorldID = 65534,
                    Flags = 15,
                    Data = Encoding.ASCII.GetBytes("\x00\x34{\"Difficulty\":1,\"Multiplayer\":3,\"PhysicsQuality\":8}\n"),
                },
                new BlobData
                {
                    Uid = Guid.Parse("3ff36c8b-93f7-4428-ae4d-429a6f0cf77d"),
                    Key = [0x1, 0x00, 0x00, 0x00],
                    WorldID = 1,
                    Flags = 13,
                    Data = new WorldData { TerrainParams = "{\"worldFile\":\"\"}\n", Classname = "CreativeFlatWorld", Seed = 1023853875, Filename = "$GAME_DATA/Scripts/game/worlds/CreativeFlatWorld.lua" }.ToBytes(),
                }
            ];


            foreach (var ply in PlayerService.Players)
            {
                var Player = ply.Value;
                var Character = CharacterService.GetCharacter(Player);

                blobs.Add(Character.BlobData(tick));
            }

            var genericInit = new GenericInitData { Data = blobs.ToArray(), GameTick = tick };
            args2.Client.Send(genericInit);

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
