using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

public struct ContainerTransaction : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.ContainerTransaction;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    /// <summary>
    /// Represents a stack of items stored in a container.
    /// </summary>
    public record struct StoredItemStack : IBitSerializable
    {
        public Guid Uuid;
        public uint InstanceId;
        public ushort Quantity;
        public ushort Slot;
        public uint ContainerId;

        public void Deserialize(ref BitReader reader)
        {
            this.Uuid = reader.ReadGuid();
            this.InstanceId = reader.ReadUInt32();
            this.Quantity = reader.ReadUInt16();
            this.Slot = reader.ReadUInt16();
            this.ContainerId = reader.ReadUInt32();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteGuid(this.Uuid);
            writer.WriteUInt32(this.InstanceId);
            writer.WriteUInt16(this.Quantity);
            writer.WriteUInt16(this.Slot);
            writer.WriteUInt32(this.ContainerId);
        }
    }

    /// <summary>
    /// Represents the type of action to perform on a container.
    /// </summary>
    public enum ActionType : byte
    {
        SetItem = 0,
        Swap = 1,
        Collect = 2,
        Spend = 3,
        CollectToSlot = 4,
        CollectToSlotOrCollect = 5,
        SpendFromSlot = 6,
        Move = 7,
        MoveFromSlot = 8,
        MoveAll = 9,
    }

    public interface IAction : IBitSerializable
    {
        /// <summary>
        /// The type of action to perform on the container.
        /// </summary>
        public abstract ActionType ActionType { get; }
    }

    public struct SetItemAction : IAction
    {
        public readonly ActionType ActionType => ActionType.SetItem;

        /// <summary>
        /// The item stack to set the slot in the container to.
        /// </summary>
        public StoredItemStack To;

        public void Deserialize(ref BitReader reader)
        {
            this.To = reader.ReadObject<StoredItemStack>();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.To);
        }
    }

    public struct SwapAction : IAction
    {
        public readonly ActionType ActionType => ActionType.Swap;

        /// <summary>
        /// The item stack of the dragged item.
        /// </summary>
        public StoredItemStack From;

        /// <summary>
        /// The item stack the dragged item is dropped onto.
        /// </summary>
        public StoredItemStack To;

        public void Deserialize(ref BitReader reader)
        {
            this.From = reader.ReadObject<StoredItemStack>();
            this.To = reader.ReadObject<StoredItemStack>();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.From);
            writer.WriteObject(this.To);
        }
    }

    public struct CollectAction : IAction
    {
        public readonly ActionType ActionType => ActionType.Collect;

        /// <summary>
        /// The UUID of the item to collect.
        /// </summary>
        public Guid Uuid;

        /// <summary>
        /// Most likely the tool instance ID, but it is always `0xFFFFFFFF`, even for tools.
        /// </summary>
        public uint ToolInstanceId;

        /// <summary>
        /// The amount of items to add to the container.
        /// </summary>
        public ushort Quantity;

        /// <summary>
        /// The ID of the container to add the items to.
        /// </summary>
        public uint ContainerId;

        /// <summary>
        /// Most likely to be the must collect all flag.
        /// </summary>
        public bool MustCollectAll;

        public void Deserialize(ref BitReader reader)
        {
            this.Uuid = reader.ReadGuid();
            this.ToolInstanceId = reader.ReadUInt32();
            this.Quantity = reader.ReadUInt16();
            this.ContainerId = reader.ReadUInt32();
            this.MustCollectAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteGuid(this.Uuid);
            writer.WriteUInt32(this.ToolInstanceId);
            writer.WriteUInt16(this.Quantity);
            writer.WriteUInt32(this.ContainerId);
            writer.WriteBoolean(this.MustCollectAll);
        }
    }

    public struct SpendAction : IAction
    {
        public ActionType ActionType => ActionType.Spend;

        /// <summary>
        /// The UUID of the item to remove from the container.
        /// </summary>
        public Guid Uuid;

        /// <summary>
        /// Most likely the tool instance ID, but it is always `0xFFFFFFFF`, even for tools.
        /// </summary>
        public uint ToolInstanceId;

        /// <summary>
        /// The amount of items to remove from the container.
        /// </summary>
        public ushort Quantity;

        /// <summary>
        /// The ID of the container to remove the items to.
        /// </summary>
        public uint ContainerId;

        /// <summary>
        /// Most likely to be the must spend all flag.
        /// </summary>
        public bool MustSpendAll;

        public void Deserialize(ref BitReader reader)
        {
            this.Uuid = reader.ReadGuid();
            this.ToolInstanceId = reader.ReadUInt32();
            this.Quantity = reader.ReadUInt16();
            this.ContainerId = reader.ReadUInt32();
            this.MustSpendAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteGuid(this.Uuid);
            writer.WriteUInt32(this.ToolInstanceId);
            writer.WriteUInt16(this.Quantity);
            writer.WriteUInt32(this.ContainerId);
            writer.WriteBoolean(this.MustSpendAll);
        }
    }

    public struct CollectToSlotAction : IAction
    {
        public readonly ActionType ActionType => ActionType.CollectToSlot;

        /// <summary>
        /// The item stack to add to the container in the specified slot.
        /// </summary>
        public StoredItemStack To;

        /// <summary>
        /// Most likely to be the must collect all flag.
        /// </summary>
        public bool MustCollectAll;

        public void Deserialize(ref BitReader reader)
        {
            this.To = reader.ReadObject<StoredItemStack>();
            this.MustCollectAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.To);
            writer.WriteBoolean(this.MustCollectAll);
        }
    }

    public struct CollectToSlotOrCollectAction : IAction
    {
        public readonly ActionType ActionType => ActionType.CollectToSlotOrCollect;

        /// <summary>
        /// The item stack to add to the container in the specified slot.
        /// </summary>
        public StoredItemStack To;

        /// <summary>
        /// Most likely to be the must collect all flag.
        /// </summary>
        public bool MustCollectAll;

        public void Deserialize(ref BitReader reader)
        {
            this.To = reader.ReadObject<StoredItemStack>();
            this.MustCollectAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.To);
            writer.WriteBoolean(this.MustCollectAll);
        }
    }

    public struct SpendFromSlotAction : IAction
    {
        public readonly ActionType ActionType => ActionType.SpendFromSlot;

        /// <summary>
        /// The item stack to remove from the container in the specified slot.
        /// </summary>
        public StoredItemStack From;

        /// <summary>
        /// Most likely to be the must spend all flag.
        /// </summary>
        public bool MustSpendAll;

        public void Deserialize(ref BitReader reader)
        {
            this.From = reader.ReadObject<StoredItemStack>();
            this.MustSpendAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.From);
            writer.WriteBoolean(this.MustSpendAll);
        }
    }

    public struct MoveAction : IAction
    {
        public readonly ActionType ActionType => ActionType.Move;

        /// <summary>
        /// The item stack to move from the source container.
        /// </summary>
        public StoredItemStack From;

        /// <summary>
        /// The item stack to move to the destination container.
        /// </summary>
        public StoredItemStack To;

        /// <summary>
        /// Most likely to be the must collect all flag.
        /// </summary>
        public bool MustCollectAll;

        public void Deserialize(ref BitReader reader)
        {
            this.From = reader.ReadObject<StoredItemStack>();
            this.To = reader.ReadObject<StoredItemStack>();
            this.MustCollectAll = reader.ReadBoolean();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteObject(this.From);
            writer.WriteObject(this.To);
            writer.WriteBoolean(this.MustCollectAll);
        }
    }

    public struct MoveFromSlotAction : IAction
    {
        public readonly ActionType ActionType => ActionType.MoveFromSlot;

        /// <summary>
        /// The slot to move the item stack from.
        /// </summary>
        public ushort SlotFrom;

        /// <summary>
        /// The ID of the container to move the item stack from.
        /// </summary>
        public uint ContainerFrom;

        /// <summary>
        /// The ID of the container to move the item stack to.
        /// </summary>
        public uint ContainerTo;

        public void Deserialize(ref BitReader reader)
        {
            this.SlotFrom = reader.ReadUInt16();
            this.ContainerFrom = reader.ReadUInt32();
            this.ContainerTo = reader.ReadUInt32();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteUInt16(this.SlotFrom);
            writer.WriteUInt32(this.ContainerFrom);
            writer.WriteUInt32(this.ContainerTo);
        }
    }

    public struct MoveAllAction : IAction
    {
        public readonly ActionType ActionType => ActionType.MoveAll;

        /// <summary>
        /// The ID of the container to move all items from.
        /// </summary>
        public uint ContainerFrom;

        /// <summary>
        /// The ID of the container to move all items to.
        /// </summary>
        public uint ContainerTo;

        public void Deserialize(ref BitReader reader)
        {
            this.ContainerFrom = reader.ReadUInt32();
            this.ContainerTo = reader.ReadUInt32();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteUInt32(this.ContainerFrom);
            writer.WriteUInt32(this.ContainerTo);
        }
    }

    /// <summary>
    /// The actions to perform on the container.
    /// </summary>
    public IAction[] Actions;

    public void Deserialize(ref BitReader reader)
    {
        var count = reader.ReadByte();
        this.Actions = new IAction[count];

        for (var i = 0; i < count; i++)
        {
            var actionType = (ActionType)reader.ReadByte();
            this.Actions[i] = actionType switch
            {
                ActionType.SetItem => new SetItemAction { },
                ActionType.Swap => new SwapAction { },
                ActionType.Collect => new CollectAction { },
                ActionType.Spend => new SpendAction { },
                ActionType.CollectToSlot => new CollectToSlotAction { },
                ActionType.CollectToSlotOrCollect => new CollectToSlotOrCollectAction { },
                ActionType.SpendFromSlot => new SpendFromSlotAction { },
                ActionType.Move => new MoveAction { },
                ActionType.MoveFromSlot => new MoveFromSlotAction { },
                ActionType.MoveAll => new MoveAllAction { },
                _ => throw new InvalidOperationException($"Unknown action type: {actionType}"),
            };
            this.Actions[i].Deserialize(ref reader);
        }
    }

    public readonly void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)this.Actions.Length);

        foreach (var action in this.Actions)
        {
            writer.WriteByte((byte)action.ActionType);
            action.Serialize(ref writer);
        }
    }
}
