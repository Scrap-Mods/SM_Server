using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Data;

public struct BlobDataRef : IBitSerializable
{
    public Guid Uid;
    public byte[] Key;

    public void Deserialize(ref BitReader reader)
    {
        Uid = reader.ReadGuid();

        var size = reader.ReadUInt16();
        Key = new byte[size];

        reader.ReadBytes(Key);
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteGuid(Uid);
        writer.WriteUInt16((UInt16)Key.Length);
        writer.WriteBytes(Key);
    }
}
