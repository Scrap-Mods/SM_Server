using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public struct BlobDataRef : IBitSerializable
{
    Guid Uid;
    UInt16 Size;
    UInt32 Key;

    public void Deserialize(ref BitReader reader)
    {
        Uid = reader.ReadGuid();
        Size = reader.ReadUInt16();
        Key = reader.ReadUInt32();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteGuid(Uid);
        writer.WriteUInt16(Size);
        writer.WriteUInt32(Key);
    }
}
