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

    public void Advance(int byteCount, int bitCount)
    {
        Seek(index + byteCount, bitOffset + bitCount);
    }

    public bool ReadBit()
    {
        byte bufferByte = buffer[index];
        bool bit = (bufferByte & (byte)(0b10000000 >> bitOffset)) != 0;
        Advance(0, 1);
        return bit;
    }

    public byte ReadByte()
    {
        if (bitOffset == 0)
        {
            var bufferByte = buffer[index];
            Advance(1, 0);
            return bufferByte;
        }
        byte byte1 = buffer[index];
        byte byte2 = buffer[index + 1];
        Advance(2, 0);
        return (byte)((byte1 << bitOffset) | (byte2 << bitOffset));
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public void ReadBytes(scoped Span<byte> destination)
    {
        if (bitOffset == 0)
        {
            if (destination.Length > buffer.Length - index)
            {
                throw new ArgumentException("Not enough bytes to fill the buffer.");
            }
            buffer.Slice(index, destination.Length).CopyTo(destination);
        }
        if (destination.Length > buffer.Length - index - 1)
        {
            throw new ArgumentException("Not enough bytes to fill the buffer.");
        }
        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = ReadByte();
        }
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

    public void ReadChars(int encodedLength, scoped Span<char> chars, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        if (bitOffset == 0)
        {
            if (encodedLength > buffer.Length - index)
            {
                throw new ArgumentException("Encoded length is bigger than the remaining byte count.");
            }
            var bytes = buffer.Slice(index, encodedLength);
            DecodeChars(bytes, chars, encoding);
        }
        else if (encodedLength <= 32)
        {
            Span<byte> bytes = stackalloc byte[encodedLength];
            ReadBytes(bytes);
            DecodeChars(bytes, chars, encoding);
        }
        else
        {
            using var bytes = bufferPool.GetBuffer(encodedLength);
            ReadBytes(bytes);
            DecodeChars(bytes, chars, encoding);
        }
    }

    public string ReadString(int encodedLength, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        if (bitOffset == 0)
        {
            if (encodedLength > buffer.Length - index)
            {
                throw new ArgumentException("Encoded length is bigger than the remaining byte count.");
            }
            var bytes = buffer.Slice(index, encodedLength);
            return DecodeString(bytes, encoding);
        }
        else if (encodedLength <= 32)
        {
            Span<byte> bytes = stackalloc byte[encodedLength];
            ReadBytes(bytes);
            return DecodeString(bytes, encoding);
        }
        else
        {
            using var bytes = bufferPool.GetBuffer(encodedLength);
            ReadBytes(bytes);
            return DecodeString(bytes, encoding);
        }
    }

    public T ReadObject<T>() where T : INetworkObject, new()
    {
        var obj = new T();
        obj.Deserialize(ref this);
        return obj;
    }

    private static void DecodeChars(ReadOnlySpan<byte> bytes, Span<char> chars, Encoding encoding)
    {
        if (!encoding.TryGetChars(bytes, chars, out _))
        {
            throw new ArgumentException("Character buffer too small.");
        }
    }

    private static string DecodeString(ReadOnlySpan<byte> bytes, Encoding encoding)
    {
        var decodedLength = encoding.GetCharCount(bytes);
        var result = new string(' ', decodedLength);
        unsafe
        {
            fixed (char* ch = result)
            {
                var chars = new Span<char>(ch, result.Length);
                encoding.TryGetChars(bytes, chars, out _);
            }
        }
        return result;
    }

    private static void ApplyByteOrder(Span<byte> bytes, ByteOrder byteOrder)
    {
        if (BitConverter.IsLittleEndian != (byteOrder != ByteOrder.BigEndian))
        {
            bytes.Reverse();
        }
    }
}
