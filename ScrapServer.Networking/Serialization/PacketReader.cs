using System.Buffers;
using System.Text;

namespace ScrapServer.Networking.Serialization;

public ref struct PacketReader
{
    public readonly int BytesLeft => buffer.Length - index - (bitIndex == 0 ? 0 : 1);

    private readonly ReadOnlySpan<byte> buffer;
    private readonly ArrayPool<byte> arrayPool;

    private int index;
    private int bitIndex;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketReader"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the raw data of the packet.</param>
    /// <param name="arrayPool">The array pool for renting temporary buffers.</param>
    public PacketReader(ReadOnlySpan<byte> buffer, ArrayPool<byte> arrayPool)
    {
        this.arrayPool = arrayPool;
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
        else
        {
            var bytes = arrayPool.Rent(encodedLength);
            ReadBytes(bytes);
            DecodeChars(bytes, chars, encoding);
            arrayPool.Return(bytes);
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
        else
        {
            var bytes = arrayPool.Rent(encodedLength);
            ReadBytes(bytes);
            var result = DecodeString(bytes, encoding);
            arrayPool.Return(bytes);
            return result;
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
            return new DecompressedData(compressed, arrayPool, decompressedLength, tryCount);
        }
        else
        {
            var compressed = arrayPool.Rent(compressedLength);
            ReadBytes(compressed);
            var result = new DecompressedData(compressed, arrayPool, decompressedLength, tryCount);
            arrayPool.Return(compressed);
            return result;
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
