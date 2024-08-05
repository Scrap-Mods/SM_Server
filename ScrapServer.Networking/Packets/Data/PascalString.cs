using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public struct PascalString : IBitSerializable
{
    public string str;

    public void Deserialize(ref BitReader reader)
    {
        var length = reader.ReadUInt16();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt16((UInt16)str.Length);
        writer.WriteBytes(Encoding.ASCII.GetBytes(str));
    }
}
