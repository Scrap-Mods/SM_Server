using OpenTK.Mathematics;
using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Core;

public class Character
{
    private static int _idCounter = 1;

    public int Id { get; private set; }
    public ulong OwnerId { get; set; }
    public uint InventoryContainerId { get; set; } = 0;
    public uint CarryContainerId { get; set; } = 0;
    public Color4 CarryContainerColor { get; set; } = Color4.White;
    public byte PlayerId { get; set; }
    public string Name { get; set; } = "MECHANIC";

    public CharacterCustomization Customization { get; set; }

    public Matrix3 HeadRotation { get; set; } = Matrix3.Identity;
    public Matrix3 BodyRotation { get; set; } = Matrix3.Identity;
    public Vector3 Position { get; set; } = new Vector3(0, 0, 0.72f);
    public Vector3 Velocity { get; set; } = Vector3.Zero;

    public float Speed { get; set; } = 0.11f;

    private Character()
    {
        Id = System.Threading.Interlocked.Increment(ref _idCounter);
        Customization = new CharacterCustomization
        {
            Gender = Gender.Male,
            Items = new List<CharacterItem>
            {
                new CharacterItem { VariantId = Guid.Parse(FaceIds.Face_1), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(HairIds.Samurai), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(FacialHairIds.Villain), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(HatsIds.Painter_Hat), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(TorsoIds.Applicator_Jacket), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(GlovesIds.Applicator_Gloves), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(LegsIds.Painter_Pants), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(ShoesIds.Painter_Shoes), PaletteIndex = 0 },
                new CharacterItem { VariantId = Guid.Parse(BackpackIds.Painter_Backpack), PaletteIndex = 0 },
            }.ToArray()
        };
    }

    private static byte ConvertAngleToBinary(double angle)
    {
        angle = (angle + Math.PI * 2) % (Math.PI * 2);

        return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public void HandleMovement(PlayerMovement movement)
    {
        // Extract velocity from movement direction
        Vector3 vector = Vector3.Zero;

        if ((movement.Keys & PlayerMovementKey.HORIZONTAL) == PlayerMovementKey.HORIZONTAL)
        {
            double direction = 2 * Math.PI * ((double)movement.Direction) / 256;
            vector = new Vector3((float)Math.Cos(direction) * Speed, (float)Math.Sin(direction) * Speed, 0);
        }

        Velocity = vector;

        // Extract rotation from yaw and pitch
        double angleYaw = 2 * Math.PI * movement.Yaw / 256;
        double anglePitch = 2 * Math.PI * movement.Pitch / 256;

        var matrixYaw = Matrix3.CreateRotationZ((float)angleYaw);
        var matrixPitch = Matrix3.CreateRotationX((float)anglePitch);

        BodyRotation = matrixYaw;
        HeadRotation = matrixYaw * matrixPitch;
    }

    public byte[] InitNetworkPacket(uint tick)
    {

        // Packet 22 - Network Update
        var stream = BitWriter.WithSharedPool();

        var anglesLook = HeadRotation.ExtractRotation().ToEulerAngles();

        var netObj = new NetObj { UpdateType = NetworkUpdateType.Create, ObjectType = NetObjType.Character, Size = 0 };
        var createUpdate = new CreateNetObj { ControllerType = ControllerType.Unknown };
        var characterCreate = new CreateCharacter
        {
            NetObjId = (uint)Id,
            SteamId = OwnerId,
            Position = Position,
            CharacterUUID = Guid.Empty,
            Pitch = ConvertAngleToBinary(anglesLook.X),
            Yaw = ConvertAngleToBinary(anglesLook.Z),
            WorldId = 1
        };

        netObj.Serialize(ref stream);
        createUpdate.Serialize(ref stream);
        characterCreate.Serialize(ref stream);
        NetObj.WriteSize(ref stream, 0);

        var streamPos = stream.ByteIndex;

        netObj = new NetObj { UpdateType = NetworkUpdateType.Update, ObjectType = NetObjType.Character, Size = 0 };
        var updateCharacter = new UpdateCharacter
        {
            NetObjId = (uint)Id,
            Color = new Color4(1, 1, 1, 1),
            Movement = new MovementState
            {
                IsDowned = false,
                IsSwimming = false,
                IsDiving = false,
                Unknown = false,
                IsClimbing = false,
                IsTumbling = false
            },
            SelectedItem = new Item { Uuid = Guid.Empty, InstanceId = -1 },
            PlayerInfo = new PlayerId { IsPlayer = true, UnitId = PlayerId }
        };

        stream.GoToNearestByte();
        netObj.Serialize(ref stream);
        updateCharacter.Serialize(ref stream);
        NetObj.WriteSize(ref stream, streamPos);

        var data = stream.Data.ToArray();

        stream.Dispose();

        return data;
    }

    public BlobData BlobData(uint tick)
    {
        var playerData = new PlayerData
        {
            CharacterID = Id,
            SteamID = OwnerId,
            InventoryContainerID = InventoryContainerId,
            CarryContainer = CarryContainerId,
            CarryColor = uint.MaxValue,
            PlayerID = (byte)(PlayerId-1),
            Name = Name,
            CharacterCustomization = Customization,
        };

        return new BlobData
        {
            Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
            Key = BitConverter.GetBytes((uint)PlayerId),
            WorldID = 65534,
            Flags = 13,
            Data = playerData.ToBytes()
        };
    }

    public BlobData BlobDataNeg(uint tick)
    {
        var playerData = new PlayerData
        {
            CharacterID = -1,
            SteamID = OwnerId,
            InventoryContainerID = InventoryContainerId,
            CarryContainer = CarryContainerId,
            CarryColor = uint.MaxValue,
            PlayerID = (byte)(PlayerId - 1),
            Name = Name,
            CharacterCustomization = Customization,
        };

        return new BlobData
        {
            Uid = Guid.Parse("51868883-d2d2-4953-9135-1ab0bdc2a47e"),
            Key = BitConverter.GetBytes((uint)PlayerId),
            WorldID = 65534,
            Flags = 13,
            Data = playerData.ToBytes()
        };
    }

    public void SpawnPackets(IClient client, uint tick)
    {
        // Packet 13 - Generic Init Data
        client.Send(new GenericInitData { Data = [BlobData(tick)], GameTick = tick });

        // Packet 22 - Network Update
        client.Send(new NetworkUpdate { GameTick = tick + 1, Updates = InitNetworkPacket(tick) });
    }

    public void RemovePackets(IClient client, uint tick)
    {
        var netObj = new RemoveNetObj
        {
            Header = new NetObj { UpdateType = NetworkUpdateType.Remove, ObjectType = NetObjType.Character, Size = 0 },
            NetObjId = (uint)Id
        };

        client.Send(new NetworkUpdate { GameTick = tick, Updates = netObj.ToBytes() });
    }

    public class Builder
    {
        private Character _character = new Character();

        public Builder WithOwnerId(ulong ownerId)
        {
            _character.OwnerId = ownerId;
            return this;
        }

        public Builder WithPlayerId(byte playerId)
        {
            _character.PlayerId = playerId;
            return this;
        }

        public Builder WithName(string name)
        {
            _character.Name = name;
            return this;
        }

        // Add more methods for other properties as needed

        public Character Build()
        {
            return _character;
        }
    }

    public static Builder CreateBuilder() => new Builder();
}

public static class CharacterService
{
    public static Dictionary<Player, Character> Characters = [];

    private static byte ConvertAngleToBinary(double angle)
    {
        angle = (angle + Math.PI * 2) % (Math.PI * 2);

        return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public static Character GetCharacter(Player player)
    {
        Character? character;
        var found = Characters.TryGetValue(player, out character);
        if (found) return character!;

        // If character isn't in Dict, we load it from the save database into the dict and return it

        // ...

        // If it still cannot be found, we make a new one
        character = Character.CreateBuilder()
            .WithOwnerId(player.SteamId)
            .WithPlayerId((byte)player.Id)
            .WithName(player.Name)
            .Build();

        Characters[player] = character;

        return character!;
    }

    public static void RemoveCharacter(Player player)
    {
        Character? character;
        _ = Characters.TryGetValue(player, out character);
        
        if (character is Character chara)
        {
            foreach (var client in PlayerService.Players.Keys)
            {
                chara.RemovePackets(client, 0);
            }

            Characters.Remove(player);
        }
    }


    //TODO(AP): Once networking is refactored, remove the "client" parameter
    public static void Tick(UInt32 tick)
    {
        var stream = BitWriter.WithSharedPool();

        foreach (var character in Characters.Values)
        {
            stream.GoToNearestByte();
            var position = stream.ByteIndex;

            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };

            character.Position += character.Velocity * character.BodyRotation;

            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = character.Id, IsTumbling = false };

            double angleMove = Math.Atan2(character.Velocity.Y, character.Velocity.X);
            var anglesLook = character.HeadRotation.ExtractRotation().ToEulerAngles();

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = character.Velocity != Vector3.Zero,
                Direction = ConvertAngleToBinary(angleMove),
                Yaw = ConvertAngleToBinary(anglesLook.Z),
                Pitch = ConvertAngleToBinary(anglesLook.X),
                Position = character.Position
            };

            netObj.Serialize(ref stream);
            updateCharacter.Serialize(ref stream);
            isNotTumbling.Serialize(ref stream);
            NetObjUnreliable.WriteSize(ref stream, position);
        }

        foreach (var client in PlayerService.Players.Keys)
        {
            client.Send(new UnreliableUpdate { CurrentTick = tick, ServerTick = tick, Updates = stream.Data.ToArray() });
        }

        stream.Dispose();
    }
}
