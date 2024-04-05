using ScrapServer.Utility.Buffers;
using System.Text;

namespace ScrapServer.Networking.Serialization;

public ref struct PacketReader
{
    private readonly BorrowedBuffer buffer;
    private readonly IBufferPool bufferPool;

    private int index;
    private byte bitOffset;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketReader"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the raw data of the packet.</param>
    /// <param name="bufferPool">The buffer pool used for borrowing additional buffers.</param>
    internal PacketReader(BorrowedBuffer buffer, IBufferPool bufferPool)
    {
        this.bufferPool = bufferPool;
        this.buffer = buffer;
    }

    public void Seek(int index, int bitOffset = 0)
    {
        this.index = index + bitOffset / 8;
        this.bitOffset = (byte)(bitOffset % 8);
    }

    public void AdvanceBits(int count)
    {
        int newBitIndex = bitOffset + count;
        index += newBitIndex / 8;
        bitOffset = (byte)(newBitIndex % 8);
    }

    public void AdvanceBytes(byte count)
    {
        index += count;
    }

    public bool ReadBit()
    {
        byte bufferByte = buffer[index];
        bool bit = (bufferByte & (byte)(0b10000000 >> bitOffset)) != 0;
        AdvanceBits(1);
        return bit;
    }

    public byte ReadByte()
    {
        if (bitOffset == 0)
        {
            var bufferByte = buffer[index];
            AdvanceBytes(1);
            return bufferByte;
        }
        byte byte1 = buffer[index];
        byte byte2 = buffer[index + 1];
        AdvanceBytes(2);
        return (byte)((byte1 << bitOffset) | (byte2 << bitOffset));
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public bool ReadBytes(scoped Span<byte> destination)
    {
        if (bitOffset == 0)
        {
            return buffer.Slice(index, destination.Length).TryCopyTo(destination);
        }
        if (destination.Length > buffer.Length - index - 1)
        {
            return false;
        }
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = ReadByte();
        }
        return true;
    }

    public bool ReadBoolean()
    {
        return ReadByte() != 0;
    }

    public ushort ReadUInt16(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt16(bytes);
    }

    public short ReadInt16(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt16(bytes);
    }

    public uint ReadUInt32(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt32(bytes);
    }

    public int ReadInt32(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt32(bytes);
    }

    public ulong ReadUInt64(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt64(bytes);
    }

    public long ReadInt64(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt64(bytes);
    }

    public float ReadSingle(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToSingle(bytes);
    }

    public double ReadDouble(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        ReadBytes(bytes);
        ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToDouble(bytes);
    }

    public Guid ReadGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        ReadBytes(bytes);
        return new Guid(bytes);
    }

    public bool TryReadChars(int encodedLength, Span<char> chars, Encoding encoding)
    {
        if (bitOffset == 0)
        {
            if (encodedLength > buffer.Length - index)
            {
                return false;
            }
            var bytes = buffer.Slice(index, encodedLength);
            return encoding.TryGetChars(bytes, chars, out _);
        }
        if (encodedLength <= 32)
        {
            Span<byte> bytes = stackalloc byte[encodedLength];
            if (!ReadBytes(bytes))
            {
                return false;
            }
            return encoding.TryGetChars(bytes, chars, out _);
        }
        else
        {
            using var buffer = bufferPool.GetBuffer(encodedLength);
            if (!ReadBytes(buffer))
            {
                return false;
            }
            return encoding.TryGetChars(buffer, chars, out _);
        }
    }

    public T ReadObject<T>() where T : INetworkObject, new()
    {
        var obj = new T();
        obj.Deserialize(ref this);
        return obj;
    }

    private static void ApplyByteOrder(Span<byte> bytes, ByteOrder byteOrder)
    {
        if (BitConverter.IsLittleEndian != (byteOrder != ByteOrder.BigEndian))
        {
            bytes.Reverse();
        }
    }
}
