﻿using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Networking.Packets.Data;


public struct UpdateUnreliableCharacter : IBitSerializable
{
    public Int32 CharacterId;
    public bool IsTumbling;

    public void Deserialize(ref BitReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteInt32(CharacterId);
        writer.WriteBit(IsTumbling);
    }
}

public struct IsNotTumbling : IBitSerializable
{
    public bool Jump;
    public bool Crawl;
    public bool Horizontal;
    public bool Sprint;
    
    public byte Direction;
    public byte Yaw;
    public byte Pitch;
    public Vector3f Position;

    public void Deserialize(ref BitReader reader)
    {
        throw new NotImplementedException();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteBit(false);
        writer.WriteBit(false);
        writer.WriteBit(false);
        writer.WriteBit(false);
        writer.WriteBit(Sprint);
        writer.WriteBit(Horizontal);
        writer.WriteBit(Crawl);
        writer.WriteBit(Jump);

        writer.WriteByte(Direction);
        writer.WriteByte(Yaw);
        writer.WriteByte(Pitch);

        Position.WriteXYZ(ref writer);
    }
}