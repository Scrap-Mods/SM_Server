using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Data;

public class CreateContainer : IBitSerializable
{
    public UInt16 StackSize;
    public ItemStack[]? Items;
    public Guid[]? Filter;

    public struct ItemStack
    {
        public Guid Uuid;
        public UInt32 InstanceId;
        public UInt16 Quantity;
    }

    public void Deserialize(ref BitReader reader)
    {
        StackSize = reader.ReadUInt16();
        Items = new ItemStack[reader.ReadUInt16()];

        for (int i = 0; i < Items.Length; i++)
        {
            Items[i] = new ItemStack
            {
                Uuid = reader.ReadGuid(),
                InstanceId = reader.ReadUInt32(),
                Quantity = reader.ReadUInt16()
            };
        }

        Filter = new Guid[reader.ReadUInt16()];

        for (int i = 0; i < Filter.Length; i++)
        {
            Filter[i] = reader.ReadGuid();
        }
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt16((UInt16)(Items?.Length ?? 0));
        writer.WriteUInt16(StackSize);

        if (Items != null)
        {
            foreach (var item in Items)
            {
                writer.WriteGuid(item.Uuid);
                writer.WriteUInt32(item.InstanceId);
                writer.WriteUInt16(item.Quantity);
            }
        }

        writer.WriteUInt16((UInt16)(Filter?.Length ?? 0));

        if (Filter != null)
        {
            foreach (var item in Filter)
            {
                writer.WriteGuid(item);
            }
        }
    }
}