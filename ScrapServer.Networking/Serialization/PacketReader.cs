using ScrapServer.Utility.Buffers;
using System.Text;

namespace ScrapServer.Networking.Serialization;

public ref struct PacketReader
{
    public readonly int BytesLeft => buffer.Length - index - (bitIndex == 0 ? 0 : 1);

    private readonly ReadOnlySpan<byte> buffer;
    private readonly IBufferPool bufferPool;

    private int index;
    private int bitIndex;

    /// <summary>
    /// The maximum length for temporary stack-allocated buffers.
    /// </summary>
    private const int StackAllocMaxLength = 32;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketReader"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the raw data of the packet.</param>
    /// <param name="bufferPool">The buffer pool used for borrowing additional buffers.</param>
    public PacketReader(ReadOnlySpan<byte> buffer, IBufferPool bufferPool)
    {
        this.bufferPool = bufferPool;
        this.buffer = buffer;
    }

    public void Seek(int index, int bitOffset = 0)
    {
        this.index = index + bitOffset / 8;
        this.bitIndex = bitOffset % 8;
    }

    public void Advance(int byteCount, int bitCount = 0)
    {
        Seek(index + byteCount, bitIndex + bitCount);
    }

    public bool ReadBit()
    {
        byte bufferByte = buffer[index];
        bool bit = (bufferByte & (0b10000000 >> bitIndex)) != 0;
        Advance(0, 1);
        return bit;
    }

    public byte ReadByte()
    {
        if (bitIndex == 0)
        {
            var bufferByte = buffer[index];
            Advance(1);
            return bufferByte;
        }
        int left = buffer[index] << bitIndex;
        int right = buffer[index + 1] >>> (8 - bitIndex);
        Advance(1);
        return unchecked((byte)(left | right));
    }

    public sbyte ReadSByte()
    {
        return unchecked((sbyte)ReadByte());
    }

    public void ReadBytes(scoped Span<byte> destination)
    {
        ThrowIfNotEnoughData(destination.Length);
        if (bitIndex == 0)
        {
            buffer.Slice(index, destination.Length).CopyTo(destination);
            Advance(destination.Length, 0);
        }
        else for (int i = 0; i < destination.Length; i++)
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

        if (bitIndex == 0)
        {
            ThrowIfNotEnoughData(encodedLength);
            var bytes = buffer.Slice(index, encodedLength);
            DecodeChars(bytes, chars, encoding);
            Advance(encodedLength);
        }
        else if (encodedLength <= StackAllocMaxLength)
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

        if (bitIndex == 0)
        {
            ThrowIfNotEnoughData(encodedLength);
            var bytes = buffer.Slice(index, encodedLength);
            Advance(encodedLength);
            return DecodeString(bytes, encoding);
        }
        else if (encodedLength <= StackAllocMaxLength)
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

    public DecompressedData ReadCompressed(int compressedLength, int decompressedLength = -1, int tryCount = 1)
    {
        decompressedLength = decompressedLength < 1 ? compressedLength * 2 : decompressedLength;

        if (bitIndex == 0)
        {
            ThrowIfNotEnoughData(compressedLength);
            var compressed = buffer.Slice(index, compressedLength);
            return new DecompressedData(bufferPool, compressed, decompressedLength, tryCount);
        }
        else if (compressedLength <= StackAllocMaxLength)
        {
            Span<byte> compressed = stackalloc byte[compressedLength];
            ReadBytes(compressed);
            return new DecompressedData(bufferPool, compressed, decompressedLength, tryCount);
        }
        else
        {
            using var compressed = bufferPool.GetBuffer(compressedLength);
            ReadBytes(compressed);
            return new DecompressedData(bufferPool, compressed, decompressedLength, tryCount);
        }
    }

    private readonly void ThrowIfNotEnoughData(int required)
    {
        if (required > BytesLeft)
        {
            throw Exceptions.NotEnoughtData;
        }
    }

    private static void DecodeChars(ReadOnlySpan<byte> bytes, Span<char> chars, Encoding encoding)
    {
        if (!encoding.TryGetChars(bytes, chars, out _))
        {
            throw Exceptions.BufferTooSmall;
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
