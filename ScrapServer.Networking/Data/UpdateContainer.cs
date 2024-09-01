using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Data;

public class UpdateContainer : IBitSerializable
{
    public struct SlotChange : IBitSerializable
    {
        public Guid Uuid;
        public UInt32 InstanceId;
        public UInt16 Quantity;
        public UInt16 Slot;

        public void Deserialize(ref BitReader reader)
        {
            Uuid = reader.ReadGuid(ByteOrder.LittleEndian);
            InstanceId = reader.ReadUInt32();
            Quantity = reader.ReadUInt16();
            Slot = reader.ReadUInt16();
        }

        public readonly void Serialize(ref BitWriter writer)
        {
            writer.WriteGuid(Uuid, ByteOrder.LittleEndian);
            writer.WriteUInt32(InstanceId);
            writer.WriteUInt16(Quantity);
            writer.WriteUInt16(Slot);
        }
    }

    public SlotChange[]? SlotChanges;
    public Guid[]? Filters;

    public void Deserialize(ref BitReader reader)
    {
        var slotChangeCount = reader.ReadInt16();
        this.SlotChanges = new SlotChange[slotChangeCount];

        for (int i = 0; i < slotChangeCount; i++)
        {
            this.SlotChanges[i] = new SlotChange();
            this.SlotChanges[i].Deserialize(ref reader);
        }

        if (reader.ReadBit())
        {
            var filterCount = reader.ReadInt16();
            this.Filters = new Guid[filterCount];

            reader.GoToNearestByte();

            for (int i = 0; i < filterCount; i++)
            {
                this.Filters[i] = reader.ReadGuid();
            }
        }
        else
        {
            this.Filters = [];
        }
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteInt16((Int16)(SlotChanges?.Length ?? 0));

        if (SlotChanges != null)
        {
            foreach (var slotChange in SlotChanges)
            {
                slotChange.Serialize(ref writer);
            }
        }

        bool hasFilters = Filters != null && Filters.Length > 0;
        writer.WriteBit(hasFilters);

        if (hasFilters)
        {
            writer.WriteInt16((Int16)(Filters!.Length));

            writer.GoToNearestByte();

            foreach (var filter in Filters)
            {
                writer.WriteGuid(filter);
            }
        }
    }
}
