using ScrapServer.Utility.Serialization;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public struct Vector3f
{
    float X;
    float Y;
    float Z;

    public void ReadXYZ(ref BitReader reader)
    {
        X = reader.ReadFloat();
        Y = reader.ReadFloat();
        Z = reader.ReadFloat();
    }

    public void WriteXYZ(ref BitWriter writer)
    {
        writer.WriteFloat(X);
        writer.WriteFloat(Y);
        writer.WriteFloat(Z);
    }
}
