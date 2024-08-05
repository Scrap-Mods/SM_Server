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
    public float X;
    public float Y;
    public float Z;

    public void ReadXYZ(ref BitReader reader)
    {
        X = reader.ReadSingle();
        Y = reader.ReadSingle();
        Z = reader.ReadSingle();
    }

    public void WriteXYZ(ref BitWriter writer)
    {
        writer.WriteSingle(X);
        writer.WriteSingle(Y);
        writer.WriteSingle(Z);
    }
}
