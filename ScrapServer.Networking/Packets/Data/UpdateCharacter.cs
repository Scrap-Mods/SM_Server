using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets.Data;
using OpenTK.Mathematics;
using ScrapServer.Networking.Packets.Utils;

public struct Item
{
    public Guid Uuid;
    public Int32 InstanceId;
}

public struct PlayerId
{
    public bool IsPlayer;
    public Int32 UnitId;
}

public struct MovementState
{
    public bool IsDowned;
    public bool IsSwimming;
    public bool IsDiving;
    public bool Unknown;
    public bool IsClimbing;
    public bool IsTumbling;
}

public struct UpdateCharacter
{
    public UInt32 NetObjId;
    public MovementState? Movement;
    public Color4? Color;
    public Item? SelectedItem;
    public PlayerId? PlayerInfo;

    public void Deserialize(ref BitReader reader)
    {
        NetObjId = reader.ReadUInt32();

        bool updateMovementState = reader.ReadBit();
        bool updateColor = reader.ReadBit();
        bool updateSelectedItem = reader.ReadBit();
        bool updateIsPlayer = reader.ReadBit();

        if (updateMovementState)
        {
            Movement = new MovementState
            {
                IsDowned = reader.ReadBit(),
                IsSwimming = reader.ReadBit(),
                IsDiving = reader.ReadBit(),
                Unknown = reader.ReadBit(),
                IsClimbing = reader.ReadBit(),
                IsTumbling = reader.ReadBit(),
            };
        }

        if (updateColor)
        {
            Color = reader.ReadColor4();
        }

        if (updateSelectedItem)
        {
            SelectedItem = new Item
            {
                Uuid = reader.ReadGuid(),
                InstanceId = reader.ReadInt32(),
            };
        }

        if (updateIsPlayer)
        {
            PlayerInfo = new PlayerId
            {
                IsPlayer = reader.ReadBit(),
                UnitId = reader.ReadInt32(),
            };
        }
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(NetObjId);

        writer.WriteBit(Movement != null);
        writer.WriteBit(Color != null);
        writer.WriteBit(SelectedItem != null);
        writer.WriteBit(PlayerInfo != null);

        if (Movement is MovementState movement)
        {
            writer.WriteBit(movement.IsDowned);
            writer.WriteBit(movement.IsSwimming);
            writer.WriteBit(movement.IsDiving);
            writer.WriteBit(movement.Unknown);
            writer.WriteBit(movement.IsClimbing);
            writer.WriteBit(movement.IsTumbling);
        }
        
        if (Color is Color4 color)
        {
            writer.WriteColor4(color);
        }

        if (SelectedItem is Item item)
        {
            writer.WriteGuid(item.Uuid);
            writer.WriteInt32(item.InstanceId);
        }

        if (PlayerInfo is PlayerId info)
        {
            writer.WriteBit(info.IsPlayer);
            writer.WriteInt32(info.UnitId);
        }
    }
}
