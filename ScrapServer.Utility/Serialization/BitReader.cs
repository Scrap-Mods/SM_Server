using System.Buffers;
using System.Text;

namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Reads non bit-aligned binary data from a <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public ref struct BitReader
{
    /// <summary>
    /// Gets the count of full bytes available for reading.
    /// </summary>
    /// <value>How many bytes are left to be read.</value>
    public readonly int BytesLeft => buffer.Length - index - (bitIndex == 0 ? 0 : 1);

    private readonly ReadOnlySpan<byte> buffer;
    private readonly ArrayPool<byte> arrayPool;

    private int index;
    private int bitIndex;

    /// <summary>
    /// Initializes a new instance of <see cref="BitReader"/>.
    /// </summary>
    /// <param name="buffer">The buffer containing the raw data.</param>
    /// <param name="arrayPool">The array pool for renting temporary buffers.</param>
    public BitReader(ReadOnlySpan<byte> buffer, ArrayPool<byte> arrayPool)
    {
        this.arrayPool = arrayPool;
        this.buffer = buffer;
    }

    /// <summary>
    /// Moves the reader forward by a specified number of bits and bytes.
    /// </summary>
    /// <param name="byteCount">The count of bytes to advance the reader by</param>
    /// <param name="bitCount">The count of bits to advance the reader by.</param>
    public void Advance(int byteCount, int bitCount = 0)
    {
        index = index + byteCount + (bitIndex + bitCount) / 8;
        bitIndex = (bitIndex + bitCount) % 8;
    }

    /// <summary>
    /// Reads a single bit and advances the reader.
    /// </summary>
    /// <returns>The read bit.</returns>
    public bool ReadBit()
    {
        byte bufferByte = buffer[index];
        bool bit = (bufferByte & BitHelper.Bit(bitIndex)) != 0;
        Advance(0, 1);
        return bit;
    }

    /// <summary>
    /// Reads a <see cref="byte"/> and advances the reader.
    /// </summary>
    /// <returns>The read <see cref="byte"/></returns>
    public byte ReadUInt8()
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

    /// <summary>
    /// Reads a <see cref="sbyte"/> and advances the reader.
    /// </summary>
    /// <returns>The read <see cref="sbyte"/></returns>
    public sbyte ReadInt8()
    {
        return unchecked((sbyte)ReadUInt8());
    }

    /// <summary>
    /// Reads bytes to fill <paramref name="destination"/> and advances the reader.
    /// </summary>
    /// <param name="destination">The buffer to read into.</param>
    public void ReadBytes(scoped Span<byte> destination)
    {
        ThrowIfNotEnoughData(destination.Length);
        if (bitIndex == 0)
        {
            buffer.Slice(index, destination.Length).CopyTo(destination);
            Advance(destination.Length, 0);
        }
        else
        {
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = ReadUInt8();
            }
        }
    }

    /// <summary>
    /// Reads a byte interpreting it as <see cref="bool"/> and advances the reader.
    /// </summary>
    /// <returns>The read <see cref="bool"/></returns>
    public bool ReadBoolean()
    {
        return ReadUInt8() != 0;
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="ushort"/></returns>
    public ushort ReadUInt16(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt16(bytes);
    }

    /// <summary>
    /// Reads a <see cref="short"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="short"/></returns>
    public short ReadInt16(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt16(bytes);
    }

    /// <summary>
    /// Reads a <see cref="uint"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="uint"/></returns>
    public uint ReadUInt32(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt32(bytes);
    }

    /// <summary>
    /// Reads a <see cref="int"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="int"/></returns>
    public int ReadInt32(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt32(bytes);
    }

    /// <summary>
    /// Reads a <see cref="ulong"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="ulong"/></returns>
    public ulong ReadUInt64(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToUInt64(bytes);
    }

    /// <summary>
    /// Reads a <see cref="long"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="long"/></returns>
    public long ReadInt64(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToInt64(bytes);
    }

    /// <summary>
    /// Reads a <see cref="float"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="float"/></returns>
    public float ReadSingle(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToSingle(bytes);
    }

    /// <summary>
    /// Reads a <see cref="double"/> and advances the reader.
    /// </summary>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    /// <returns>The read <see cref="double"/></returns>
    public double ReadDouble(ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        ReadBytes(bytes);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        return BitConverter.ToDouble(bytes);
    }

    /// <summary>
    /// Reads a <see cref="Guid"/> and advances the reader.
    /// </summary>
    /// <returns>The read <see cref="Guid"/>.</returns>
    public Guid ReadGuid()
    {
        Span<byte> bytes = stackalloc byte[16];
        ReadBytes(bytes);
        return new Guid(bytes);
    }

    /// <summary>
    /// Reads encoded text of the specified length into a <see cref="char"/> buffer and advances the reader.
    /// </summary>
    /// <param name="encodedLength">The length of the encoded text in bytes.</param>
    /// <param name="chars">The buffer to write the decoded text into.</param>
    /// <param name="encoding">The text encoding (<see cref="Encoding.UTF8"/> by default).</param>
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

    /// <summary>
    /// Reads a text string of the specified length and advances the reader.
    /// </summary>
    /// <param name="encodedLength">The length of the encoded text in bytes.</param>
    /// <param name="encoding">The text encoding (<see cref="Encoding.UTF8"/> by default).</param>
    /// <returns>The read <see cref="string"/>.</returns>
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

    /// <summary>
    /// Reads a serializable object and advances the reader.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The read object.</returns>
    public T ReadObject<T>() where T : IBitSerializable, new()
    {
        var obj = new T();
        obj.Deserialize(ref this);
        return obj;
    }

    /// <summary>
    /// Reads and decompresses a blob of LZ4 compressed data and advances the reader.
    /// </summary>
    /// <param name="compressedLength">The length of the blob.</param>
    /// <param name="decompressedLength">
    /// The length of the decompressed data (estimated to be 2 * <paramref name="compressedLength"/> by default).
    /// </param>
    /// <param name="tryCount">
    /// The max number of tries decompression will be attempted with an increased buffer size.
    /// </param>
    /// <returns>The decompressed data that can be read with <see cref="DecompressedData.Reader"/>.</returns>
    public DecompressedData ReadLZ4(int compressedLength, int decompressedLength = -1, int tryCount = 3)
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
}
