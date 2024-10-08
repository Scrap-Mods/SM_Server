﻿using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Core.Utils;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;
using System.Text;
using ScrapServer.Core.NetObjs;


namespace ScrapServer.Core;

public class Player
{
    public uint Id;
    public ulong SteamId;
    public string Username = "";
    public Character? Character { get; private set; }
    public Connection? SteamConn { get; private set; }

    public Player(Connection conn)
    {
        SteamConn = conn;
    }

    public void Kick()
    {
        SteamConn?.Close();
        SteamConn = null;
    }

    public void Send<T>(T data) where T : IPacket, new()
    {
        var writer = BitWriter.WithSharedPool();
        writer.WritePacket<T>(data);

        unsafe
        {
            fixed (byte* ptr = writer.Data)
            {
                SteamConn?.SendMessage((nint)ptr, writer.Data.Length, SendType.Reliable);
            }
        }

        writer.Dispose();
    }

    public void Receive(ReadOnlySpan<byte> data)
    {
        var id = (PacketId) data[0];
        var reader = BitReader.WithSharedPool(data);
 
        if (id == PacketId.Hello)
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

            foreach (var ply in PlayerService.GetPlayers())
            {
                if (ply.Character == null) continue;

                blobDataRefs.Add(new BlobDataRef {
                    Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
                    Key = BitConverter.GetBytes(ply.Id),
                });
            }

            Send(new ScrapServer.Networking.ServerInfo
            {
                Version = 729,
                Gamemode = Gamemode.FlatTerrain,
                Seed = 1023853875,
                GameTick = SchedulerService.GameTick,
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


            foreach (var ply in PlayerService.GetPlayers())
            {
                if (ply.Character == null) continue;

                blobs.Add(ply.Character.BlobData(SchedulerService.GameTick));
            }

            var genericInit = new GenericInitData { Data = blobs.ToArray(), GameTick = SchedulerService.GameTick };
            Send(genericInit);
        }
        else if (id == PacketId.FileChecksums)
        {
            Send(new ChecksumsAccepted {});
        }
        else if (id == PacketId.CharacterInfo)
        {
            var info = reader.ReadPacket<CharacterInfo>();

            Console.WriteLine("Received CharacterInfo");

            Send(new JoinConfirmation { });

            Console.WriteLine("Sent JoinConfirmation");

            var character = CreateCharacter();

            character.Name = info.Name ?? "MECHANIC";
            character.Customization = info.Customization;

            // Send Initialization Network Update
            List<byte> bytes = [];
            foreach (var ply in PlayerService.GetPlayers())
            {
                if (ply.Character == null) continue;

                bytes.AddRange(ply.Character.InitNetworkPacket(SchedulerService.GameTick));
            }

            Send(new InitNetworkUpdate { GameTick = SchedulerService.GameTick, Updates = bytes.ToArray() });
            Send(new ScriptDataS2C { GameTick = SchedulerService.GameTick, Data = [] });

            foreach (var client in PlayerService.GetPlayers())
            {
                character.SpawnPackets(client, SchedulerService.GameTick);
            }

            Console.WriteLine("Sent ScriptInitData and NetworkInitUpdate for Client");
        }
        else if (id == PacketId.PlayerMovement)
        {
            var packet = reader.ReadPacket<PlayerMovement>();

            Console.WriteLine("Player: {0}, {1} Character: {2}", Id.ToString("D"), Id.ToString("D"), CharacterService.GetCharacter(this).Id);
            
            Character?.HandleMovement(packet);
        }
        else if (id == PacketId.Broadcast)
        {
            var packet = reader.ReadPacket<Broadcast>();

            foreach (var player in PlayerService.GetPlayers())
            {
                if (player != this)
                {
                    player.Send(packet);
                }
            }
        }
    }

    public Character CreateCharacter()
    {
        Character = CharacterService.GetCharacter(this);

        return Character;
    }

    public void RemoveCharacter()
    {
        CharacterService.RemoveCharacter(this);
        Character = null;
    }
}

public static class PlayerService
{
    private class SocketInterface : ISocketManager
    {
        public void OnConnecting(Connection connection, ConnectionInfo info)
        {
            connection.Accept();
        }

        public void OnConnected(Connection connection, ConnectionInfo info)
        {
            var player = CreatePlayer(connection, info.Identity);
            player.Send(new ClientAccepted());
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            RemovePlayer(connection);
        }

        public void OnMessage(Connection connection, NetIdentity identity, nint data, int size, long messageNum, long recvTime, int channel)
        {
            var player = GetPlayer(connection);

            ReadOnlySpan<byte> dataSpan;
            unsafe
            {
                dataSpan = new ReadOnlySpan<byte>((void*)data, size);
            }

            player.Receive(dataSpan);
        }
    }

    private static SocketManager? socketP2P;
    private static SocketManager? socketIP;
    
    private static Dictionary<Connection, Player> Players = [];
    private static uint NextPlayerID = 1;


    public static Player[] GetPlayers()
    {
        return Players.Values.ToArray();
    }

    public static void Tick()
    {
        socketIP?.Receive();
        socketP2P?.Receive();
    }

    public static void Init()
    {
        socketP2P = SteamNetworkingSockets.CreateRelaySocket<SocketManager>();
        socketIP = SteamNetworkingSockets.CreateNormalSocket<SocketManager>(NetAddress.AnyIp(38799));

        var inter = new SocketInterface();

        socketP2P.Interface = inter;
        socketIP.Interface = inter;
    }

    private static Player CreatePlayer(Connection conn, NetIdentity identity)
    {
        // First check database and load player from it into the dict and return it

        // ...

        // If it still cannot be found, we make a new one
        var player = new Player(conn);

        player.Id = NextPlayerID;
        player.SteamId = identity.SteamId.Value;
        player.Username = new Friend(identity.SteamId).Name;

        Players[conn] = player;
        NextPlayerID += 1;

        return player;
    }

    private static Player GetPlayer(Connection conn)
    {
        return Players[conn];
    }

    private static void RemovePlayer(Connection conn)
    {
        Player? player;
        var found = Players.TryGetValue(conn, out player);

        if (player is Player ply)
        {
            CharacterService.RemoveCharacter(ply);
            Players.Remove(conn);
        }
    }
}
