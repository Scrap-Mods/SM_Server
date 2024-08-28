using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Core.NetObjs;

public class Container : INetObj
{
    public NetObjType NetObjType => NetObjType.Container;
    public ControllerType ControllerType => ControllerType.Unknown;

    public uint Id { get; private set; }
    public readonly ushort MaximumStackSize;
    public readonly ItemStack[] Items;
    public readonly ISet<Guid> Filter = new HashSet<Guid>();


    public record ItemStack(Guid Uuid, uint InstanceId, ushort Quantity)
    {
        public const uint NoInstanceId = uint.MaxValue;

        public static ItemStack Empty => new(Uuid: Guid.Empty, InstanceId: NoInstanceId, Quantity: 0);

        public bool IsEmpty => Uuid == Guid.Empty || Quantity == 0;

        public bool IsStackableWith(ItemStack other)
        {
            if (other == null) return false;

            if (this.IsEmpty || other.IsEmpty) return true;

            return this.Uuid == other.Uuid && this.InstanceId == other.InstanceId;
        }

        public static ItemStack Combine(ItemStack a, ItemStack b)
        {
            if (!a.IsStackableWith(b))
            {
                throw new InvalidOperationException("Cannot add two ItemStacks that are not stackable");
            }

            var quantity = a.Quantity + b.Quantity;

            if (quantity > ushort.MaxValue)
            {
                throw new InvalidOperationException("Cannot add two ItemStacks that would overflow the quantity");
            }

            var baseItemStack = !a.IsEmpty ? a : b;

            return baseItemStack with { Quantity = (ushort)quantity };
        }
    }

    public Container(uint id, ushort size, ushort maximumStackSize = ushort.MaxValue)
    {
        this.Id = id;
        this.MaximumStackSize = maximumStackSize;
        this.Items = Enumerable.Repeat(ItemStack.Empty, size).ToArray();
    }

    public Container Clone()
    {
        var clone = new Container(this.Id, (ushort)Items.Length, this.MaximumStackSize);
        for (int i = 0; i < this.Items.Length; i++)
        {
            clone.Items[i] = this.Items[i];
        }

        clone.Filter.UnionWith(this.Filter);

        return clone;
    }

    public void SerializeCreate(ref BitWriter writer)
    {
        new CreateContainer
        {
            StackSize = this.MaximumStackSize,
            Items = this.Items.Select(item => new CreateContainer.ItemStack
            {
                Uuid = item.Uuid,
                InstanceId = item.InstanceId,
                Quantity = item.Quantity
            }).ToArray(),
            Filter = [.. this.Filter],
        }.Serialize(ref writer);
    }

    /// <summary>
    /// Serialize the update of the container with no changes.
    /// </summary>
    /// <param name="writer"><inheritdoc/></param>
    public void SerializeUpdate(ref BitWriter writer)
    {
        new UpdateContainer
        {
            SlotChanges = [],
            Filters = [],
        }.Serialize(ref writer);
    }

    /// <summary>
    /// Create an update of the container based on the old state.
    /// </summary>
    /// <param name="oldState">The cloned old state of the container</param>
    /// <returns>The update of the container</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public UpdateContainer CreateNetworkUpdate(Container oldState)
    {
        ArgumentNullException.ThrowIfNull(oldState);

        if (this.Id != oldState.Id)
        {
            throw new InvalidOperationException("Cannot serialize update with different container ids");
        }

        if (this.Items.Length != oldState.Items.Length)
        {
            throw new InvalidOperationException("Cannot serialize update with different item counts");
        }

        var slotChanges = new List<UpdateContainer.SlotChange>();

        for (int i = 0; i < this.Items.Length; i++)
        {
            var item = this.Items[i];
            var oldItem = oldState.Items[i];

            if (item != oldItem)
            {
                slotChanges.Add(new UpdateContainer.SlotChange
                {
                    Uuid = item.Uuid,
                    InstanceId = item.InstanceId,
                    Quantity = item.Quantity,
                    Slot = (ushort)i,
                });
            }
        }

        Guid[] filterChanges = this.Filter.Equals(oldState.Filter) ? [] : [.. this.Filter];

        return new UpdateContainer
        {
            SlotChanges = [.. slotChanges],
            Filters = filterChanges,
        };
    }
}
