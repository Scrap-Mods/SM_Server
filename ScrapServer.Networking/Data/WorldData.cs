using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Data;

public struct WorldData : IBitSerializable
{
    public UInt32 Seed;
    public String Filename;
    public String Classname;
    public String TerrainParams;

    public void Deserialize(ref BitReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(Seed);
        writer.WriteUInt16((UInt16)Filename.Length);
        writer.WriteBytes(Encoding.ASCII.GetBytes(Filename));
        writer.WriteUInt16((UInt16)Classname.Length);
        writer.WriteBytes(Encoding.ASCII.GetBytes(Classname));
        writer.WriteUInt16((UInt16)TerrainParams.Length);
        writer.WriteBytes(Encoding.ASCII.GetBytes(TerrainParams));
    }
}
