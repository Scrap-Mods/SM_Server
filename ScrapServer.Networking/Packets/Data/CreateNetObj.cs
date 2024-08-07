using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;

public struct CreateNetObj : IBitSerializable
{
    public ControllerType ControllerType;

    public void Deserialize(ref BitReader reader)
    {
        ControllerType = (ControllerType)reader.ReadByte();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteByte((byte)ControllerType);
    }
}