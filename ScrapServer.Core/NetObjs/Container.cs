using System.Drawing;

namespace ScrapServer.Core.NetObjs;

public class Container
{
    public readonly uint Id;
    public readonly ushort MaximumStackSize;
    public readonly ItemStack[] Items;

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
        var clone = new Container(this.Id, (ushort)Items.Length);
        for (int i = 0; i < this.Items.Length; i++)
        {
            clone.Items[i] = this.Items[i];
        }

        return clone;
    }
}
