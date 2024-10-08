﻿using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Networking.Utils;

internal static class BitReaderExtensions
{
    public static Gamemode ReadGamemode(this ref BitReader reader)
    {
        return (Gamemode)reader.ReadUInt32();
    }

    public static ServerFlags ReadServerFlags(this ref BitReader reader)
    {
        return (ServerFlags)reader.ReadByte();
    }

    public static Vector3 ReadVector3XYZ(this ref BitReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }

    public static Vector3 ReadVector3ZYX(this ref BitReader reader)
    {

        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();

        return new Vector3(z, y, x);
    }

    public static Color4 ReadColor4(this ref BitReader reader)
    {
        var r = reader.ReadByte() / 0xFF;
        var g = reader.ReadByte() / 0xFF;
        var b = reader.ReadByte() / 0xFF;
        var a = reader.ReadByte() / 0xFF;

        return new Color4(r, g, b, a);
    }
}
