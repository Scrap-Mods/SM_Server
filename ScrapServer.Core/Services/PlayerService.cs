using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Core.Utils;
using ScrapServer.Utility.Serialization;
using Steamworks;
using Steamworks.Data;
using System.Text;
using ScrapServer.Core.NetObjs;
using static ScrapServer.Core.NetObjs.Container;

namespace ScrapServer.Core;

public class Player
{
    public uint Id;
    public ulong SteamId;
    public string Username = "";
    public Character? Character { get; private set; }
    public Container InventoryContainer { get; private set; }
    public Container CarryContainer { get; private set; }
    public Connection? SteamConn { get; private set; }

    public Player(Connection conn, Container inventoryContainer, Container carryContainer)
    {
        SteamConn = conn;
        InventoryContainer = inventoryContainer;
        CarryContainer = carryContainer;
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
                Version = 723,
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


            foreach (var player in PlayerService.GetPlayers())
            {
                if (player.Character == null) continue;

                blobs.Add(player.GetPlayerData(player.Character));
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
            var builder = new NetworkUpdate.Builder();
            foreach (var player in PlayerService.GetPlayers())
            {
                if (player.Character == null) continue;

                builder.WriteCreate(player.Character);
                builder.WriteUpdate(player.Character);
                builder.WriteCreate(player.InventoryContainer);
                builder.WriteCreate(player.CarryContainer);
            }

            Send(new InitNetworkUpdate { GameTick = SchedulerService.GameTick, Updates = builder.Build().Updates });
            Send(new ScriptDataS2C { GameTick = SchedulerService.GameTick, Data = [] });

            foreach (var client in PlayerService.GetPlayers())
            {
                client.SendSpawnPackets(this, character, SchedulerService.GameTick);
            }

            Console.WriteLine("Sent ScriptInitData and NetworkInitUpdate for Client");
        }
        else if (id == PacketId.PlayerMovement)
        {
            var packet = reader.ReadPacket<PlayerMovement>();

            Console.WriteLine("Player: {0}, {1} Character: {2}", Id.ToString("D"), Id.ToString("D"), CharacterService.GetCharacter(this).Id);
            
            Character?.HandleMovement(packet);
        }
        else if (id == PacketId.ContainerTransaction)
        {
            var packet = reader.ReadPacket<ContainerTransaction>();
            var containerService = ContainerService.Instance;

            using var transaction = containerService.BeginTransaction();

            foreach (var action in packet.Actions)
            {
                switch (action)
                {
                    case ContainerTransaction.SetItemAction setItemAction:
                        {
                            if (
                                !containerService.Containers.TryGetValue(setItemAction.To.ContainerId, out var containerTo) ||
                                setItemAction.To.Slot >= containerTo.Items.Length)
                            {
                                break;
                            }
                            transaction.SetItem(containerTo, setItemAction.To.Slot, new ItemStack(
                                setItemAction.To.Uuid,
                                setItemAction.To.InstanceId,
                                setItemAction.To.Quantity
                            ));
                        }
                        break;

                    case ContainerTransaction.SwapAction swapAction:
                        {
                            if (
                                !containerService.Containers.TryGetValue(swapAction.From.ContainerId, out var containerFrom) ||
                                !containerService.Containers.TryGetValue(swapAction.To.ContainerId, out var containerTo) ||
                                swapAction.From.Slot >= containerFrom.Items.Length ||
                                swapAction.To.Slot >= containerTo.Items.Length)
                            {
                                break;
                            }
                            transaction.Swap(containerFrom, swapAction.From.Slot, containerTo, swapAction.To.Slot);
                        }
                        break;

                    case ContainerTransaction.CollectAction collectAction:
                    case ContainerTransaction.SpendAction spendAction:
                    case ContainerTransaction.CollectToSlotAction collectToSlotAction:
                    case ContainerTransaction.CollectToSlotOrCollectAction collectToSlotOrCollectAction:
                    case ContainerTransaction.SpendFromSlotAction spendFromSlotAction:
                        throw new NotImplementedException($"Container transaction action {action} not implemented");

                    case ContainerTransaction.MoveAction moveAction:
                        {
                            if (
                                !containerService.Containers.TryGetValue(moveAction.From.ContainerId, out var containerFrom) ||
                                !containerService.Containers.TryGetValue(moveAction.To.ContainerId, out var containerTo) ||
                                moveAction.From.Slot >= containerFrom.Items.Length ||
                                moveAction.To.Slot >= containerTo.Items.Length)
                            {
                                break;
                            }
                            transaction.Move(containerFrom, moveAction.From.Slot, containerTo, moveAction.To.Slot, moveAction.From.Quantity, moveAction.MustCollectAll);
                        }
                        break;

                    case ContainerTransaction.MoveFromSlotAction moveFromSlotAction:
                        {
                            if (
                                !containerService.Containers.TryGetValue(moveFromSlotAction.ContainerFrom, out var containerFrom) ||
                                !containerService.Containers.TryGetValue(moveFromSlotAction.ContainerTo, out var containerTo) ||
                                moveFromSlotAction.SlotFrom >= containerFrom.Items.Length)
                            {
                                break;
                            }
                            transaction.MoveFromSlot(containerFrom, moveFromSlotAction.SlotFrom, containerTo);
                        }
                        break;

                    case ContainerTransaction.MoveAllAction moveAllAction:
                        throw new NotImplementedException($"Container transaction action {action} not implemented");

                    default:
                        return;
                }
            }

            var networkUpdate = new NetworkUpdate.Builder()
                .WithGameTick(SchedulerService.GameTick);

            foreach (var (container, update) in transaction.EndTransaction())
            {
                Console.WriteLine("Sending container update for container {0}", container.Id);
                networkUpdate.Write(container, NetworkUpdateType.Update, (ref BitWriter writer) => update.Serialize(ref writer));
            }

            var updatePacket = networkUpdate.Build();
            foreach (var player in PlayerService.GetPlayers())
            {
                player.Send(updatePacket);
            }
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

    public BlobData GetPlayerData(Character character)
    {
        var playerData = new PlayerData
        {
            CharacterID = (int)character.Id,
            SteamID = this.SteamId,
            InventoryContainerID = this.InventoryContainer.Id,
            CarryContainer = this.CarryContainer.Id,
            CarryColor = uint.MaxValue,
            PlayerID = (byte)(this.Id - 1),
            Name = this.Username,
            CharacterCustomization = character.Customization,
        };

        return new BlobData
        {
            Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
            Key = BitConverter.GetBytes((uint)this.Id),
            WorldID = 65534,
            Flags = 13,
            Data = playerData.ToBytes()
        };
    }
    public void SendSpawnPackets(Player player, Character character, uint tick)
    {
        // Packet 13 - Generic Init Data
        this.Send(new GenericInitData { Data = [player.GetPlayerData(character)], GameTick = tick });

        // Packet 22 - Network Update
        this.Send(
            new NetworkUpdate.Builder()
                .WithGameTick(tick + 1)
                .WriteCreate(character)
                .WriteUpdate(character)
                .Build()
        );
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
        var inventoryContainer = ContainerService.Instance.CreateContainer(30);
        Console.WriteLine("Created inventory container with ID {0}", inventoryContainer.Id);
        using (var transaction = ContainerService.Instance.BeginTransaction())
        {
            for (int i = 0; i < 30; i++)
            {
                transaction.CollectToSlot(
                    inventoryContainer,
                    new ItemStack(Guid.Parse(i % 2 == 0 ? "df953d9c-234f-4ac2-af5e-f0490b223e71" : "a6c6ce30-dd47-4587-b475-085d55c6a3b4"), ItemStack.NoInstanceId, (ushort)(i + 1)),
                    (ushort)i
                );
            }
            Console.WriteLine("Collected items into inventory container");
            transaction.EndTransaction();
        }

        var carryContainer = ContainerService.Instance.CreateContainer(1);
        Console.WriteLine("Created carry container with ID {0}", carryContainer.Id);
        var player = new Player(conn, inventoryContainer, carryContainer);
        Console.WriteLine("Created player with ID {0}", player.Id);

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
