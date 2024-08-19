using OpenTK.Mathematics;
using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Core.NetObjs;

public class Character : INetObj
{
    public NetObjType NetObjType => NetObjType.Character;
    public ControllerType ControllerType => ControllerType.Unknown;

    private static uint _idCounter = 1;

    public uint Id { get; private set; }
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
    public Vector3 TargetVelocity { get; set; } = Vector3.Zero;

    public bool IsSlowedDown { get; set; } = false;
    public bool IsCrouching { get; set; } = false;
    public bool IsSprinting { get; set; } = false;

    public float WalkSpeed { get; set; } = 4f;
    public float SlowDownSpeed { get; set; } = 2.5f;
    public float CrouchSpeed { get; set; } = 3f;
    public float SprintSpeed { get; set; } = 8f;

    public float StandHeight { get; set; } = 1.4f;
    public float CrouchHeight { get; set; } = 0.8f;

    public float MovementSpeed
    {
        get
        {
            if (IsSprinting)
            {
                return SprintSpeed;
            }
            else if (IsSlowedDown)
            {
                return SlowDownSpeed;
            }
            else if (IsCrouching)
            {
                return CrouchSpeed;
            }
            else
            {
                return WalkSpeed;
            }
        }
    }

    public float Height
    {
        get
        {
            if (IsCrouching)
            {
                return CrouchHeight;
            }
            else
            {
                return StandHeight;
            }
        }
    }

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

        IsSlowedDown = (movement.Keys & PlayerMovementKey.SLOW_DOWN) != 0;
        IsSprinting = (movement.Keys & PlayerMovementKey.SPRINT) != 0;

        var newIsCrouching = (movement.Keys & PlayerMovementKey.CRAWL) != 0;
        if (newIsCrouching != IsCrouching)
        {
            var heightDifferenceWhenCrouching = StandHeight / 2f - CrouchHeight / 2f;
            if (newIsCrouching)
            {
                Position = new Vector3(Position.X, Position.Y, Position.Z - heightDifferenceWhenCrouching);
            }
            else
            {
                Position = new Vector3(Position.X, Position.Y, Position.Z + heightDifferenceWhenCrouching);
            }
            IsCrouching = newIsCrouching;
        }

        if ((movement.Keys & PlayerMovementKey.HORIZONTAL) != 0)
        {
            double direction = 2 * Math.PI * ((double)movement.Direction) / 256;
            vector = new Vector3((float)Math.Cos(direction) * MovementSpeed, (float)Math.Sin(direction) * MovementSpeed, 0);
        }

        TargetVelocity = vector;

        // Extract rotation from yaw and pitch
        double angleYaw = 2 * Math.PI * movement.Yaw / 256;
        double anglePitch = 2 * Math.PI * movement.Pitch / 256;

        var matrixYaw = Matrix3.CreateRotationZ((float)angleYaw);
        var matrixPitch = Matrix3.CreateRotationX((float)anglePitch);

        BodyRotation = matrixYaw;
        HeadRotation = matrixYaw * matrixPitch;
    }
    public void SerializeCreate(ref BitWriter writer)
    {
        var anglesLook = HeadRotation.ExtractRotation().ToEulerAngles();

        new CreateCharacter
        {
            SteamId = OwnerId,
            Position = Position,
            CharacterUUID = Guid.Empty,
            Pitch = ConvertAngleToBinary(anglesLook.X),
            Yaw = ConvertAngleToBinary(anglesLook.Z),
            WorldId = 1
        }.Serialize(ref writer);
    }

    public void SerializeUpdate(ref BitWriter writer)
    {
        new UpdateCharacter
        {
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
        }.Serialize(ref writer);
    }

    public BlobData BlobData(uint tick)
    {
        var playerData = new PlayerData
        {
            CharacterID = (int)Id,
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

    public void SpawnPackets(Player player, uint tick)
    {
        // Packet 13 - Generic Init Data
        player.Send(new GenericInitData { Data = [BlobData(tick)], GameTick = tick });

        // Packet 22 - Network Update
        player.Send(
            new NetworkUpdate.Builder()
                .WithGameTick(tick + 1)
                .WriteCreate(this)
                .WriteUpdate(this)
                .Build()
        );
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
