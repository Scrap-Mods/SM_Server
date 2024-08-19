using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ScrapServer.Utility.Serialization;

/// <summary>
/// Writes non bit-aligned binary data to an array.
/// </summary>
public struct BitWriter : IDisposable
{
    /// <summary>
    /// Gets the current byte position of the writer.
    /// </summary>
    /// <value>Current byte index.</value>
    public readonly int ByteIndex => byteIndex;

    /// <summary>
    /// Gets the current bit position of the writer.
    /// </summary>
    /// <value>Current bit index from 0 to 8.</value>
    public readonly int BitIndex => bitIndex;

    /// <summary>
    /// Gets the written data.
    /// </summary>
    /// <value>The span containing the written data.</value>
    public readonly ReadOnlySpan<byte> Data => new ReadOnlySpan<byte>(buffer, 0, byteIndex + (bitIndex + 7) / 8);

    private readonly ArrayPool<byte> arrayPool;
    private byte[] buffer;

    private int byteIndex;
    private int bitIndex;

    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of <see cref="BitWriter"/>.
    /// </summary>
    /// <param name="arrayPool">The array pool for renting buffers for writing.</param>
    public BitWriter(ArrayPool<byte> arrayPool)
    {
        this.arrayPool = arrayPool;
        buffer = Array.Empty<byte>();
    }

    /// <summary>
    /// Creates a new <see cref="BitWriter"/> instance using <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    /// <returns>The created <see cref="BitWriter"/>.</returns>
    public static BitWriter WithSharedPool()
    {
        return new BitWriter(ArrayPool<byte>.Shared);
    }

    /// <summary>
    /// Moves the writer to the specified position.
    /// </summary>
    /// <param name="byteIndex">The byte index.</param>
    /// <param name="bitIndex">The bit index.</param>
    public void Seek(int byteIndex, int bitIndex = 0)
    {
        this.byteIndex = byteIndex + bitIndex / 8;
        this.bitIndex = bitIndex % 8;
    }

    /// <summary>
    /// Moves the writer forward by the specified number of bits and bytes.
    /// </summary>
    /// <param name="byteCount">The numbers of bytes.</param>
    /// <param name="bitCount">The number of bits.</param>
    public void Advance(int byteCount, int bitCount = 0)
    {
        Seek(byteIndex + byteCount, bitIndex + bitCount);
    }

    /// <summary>
    /// Advances the writer to the nearest beginning of byte.
    /// </summary>
    public void GoToNearestByte()
    {
        if (bitIndex != 0)
        {
            Advance(0, 8 - bitIndex);
        }
    }

    /// <summary>
    /// Preallocates the specified number of bits and bytes.
    /// </summary>
    /// <param name="byteCount">The number of bytes.</param>
    /// <param name="bitCount">The number of bits.</param>
    public void EnsureCapacity(int byteCount, int bitCount = 0)
    {
        int oldLength = byteIndex + (bitIndex + 7) / 8;
        int newLength = byteCount + (bitCount + 7) / 8;
        if (buffer.Length >= newLength)
        {
            return;
        }
        var newBuffer = arrayPool.Rent(newLength);
        Array.Copy(buffer, 0, newBuffer, 0, oldLength);
        Array.Fill<byte>(newBuffer, 0, oldLength, newLength - oldLength);
        arrayPool.Return(buffer);
        buffer = newBuffer;
    }

    /// <summary>
    /// Preallocates the specified number of bits and bytes on top of existing capacity.
    /// </summary>
    /// <param name="byteCount">The number of extra bytes.</param>
    /// <param name="bitCount">The number of extra bits.</param>
    public void EnsureAdditionalCapacity(int byteCount, int bitCount = 0)
    {
        EnsureCapacity(byteIndex + byteCount, bitIndex + bitCount);
    }

    /// <summary>
    /// Writes a single bit.
    /// </summary>
    /// <param name="value">The value of the bit.</param>
    public void WriteBit(bool value)
    {
        EnsureAdditionalCapacity(0, 1);
        if (value)
        {
            buffer[byteIndex] = unchecked((byte)(buffer[byteIndex] | BitHelper.Bit(bitIndex)));
        }
        else
        {
            buffer[byteIndex] = unchecked((byte)(buffer[byteIndex] & ~BitHelper.Bit(bitIndex)));
        }
        Advance(0, 1);
    }

    /// <summary>
    /// Writes a <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteByte(byte value)
    {
        EnsureAdditionalCapacity(1);
        if (bitIndex == 0)
        {
            buffer[byteIndex] = value;
        }
        else
        {
            int oldMask = BitHelper.LeftBitMask(bitIndex);
            int oldLeft = buffer[byteIndex] & oldMask;
            int oldRight = buffer[byteIndex + 1] & ~oldMask;

            int newLeft = value >>> bitIndex;
            int newRight = value << (8 - bitIndex);

            buffer[byteIndex] = unchecked((byte)(oldLeft | newLeft));
            buffer[byteIndex + 1] = unchecked((byte)(oldRight | newRight));
        }
        Advance(1);
    }

    /// <summary>
    /// Writes a <see cref="sbyte"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteSByte(sbyte value)
    {
        WriteByte(unchecked((byte)value));
    }

    /// <summary>
    /// Writes an array of bytes.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        EnsureAdditionalCapacity(bytes.Length);
        if (bitIndex == 0)
        {
            bytes.CopyTo(buffer.AsSpan(byteIndex, bytes.Length));
            Advance(bytes.Length);
        }
        else
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                WriteByte(bytes[i]);
            }
        }
    }

    /// <summary>
    /// Writes a boolean value as a byte (<see langword="true"/> as <c>0x01</c>, <see langword="false"/> as <c>0x00</c>).
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteBoolean(bool value)
    {
        WriteByte((byte)(value ? 1 : 0));
    }

    /// <summary>
    /// Writes a <see cref="ushort"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteUInt16(ushort value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="short"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteInt16(short value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(short)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="uint"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteUInt32(uint value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="int"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteInt32(int value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="ulong"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteUInt64(ulong value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="long"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteInt64(long value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="float"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteSingle(float value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(float)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="double"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="byteOrder">The endianness of the binary encoded number.</param>
    public void WriteDouble(double value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[sizeof(double)];
        BitConverter.TryWriteBytes(bytes, value);
        BitHelper.ApplyByteOrder(bytes, byteOrder);
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a <see cref="Guid"/>.
    /// </summary>
    /// <param name="value">The <see cref="Guid"/> to write.</param>
    /// <param name="byteOrder">The endianness of the encoded fields of the <see cref="Guid"/>.</param>
    public void WriteGuid(Guid value, ByteOrder byteOrder = ByteOrder.BigEndian)
    {
        Span<byte> bytes = stackalloc byte[16];
        
        // Always write the Guid in big endian, and later reverse if needed,
        // as serializing a Guid in little endian does not produce a valid RFC 4122 Uuid.
        value.TryWriteBytes(bytes, true, out _);

        if (byteOrder == ByteOrder.LittleEndian)
        {
            bytes.Reverse();
        }

        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a string of characters.
    /// </summary>
    /// <param name="chars">The characters to write.</param>
    /// <param name="encoding">The text encoding (<see cref="Encoding.UTF8"/> by default).</param>
    public void WriteChars(ReadOnlySpan<char> chars, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        var byteCount = encoding.GetByteCount(chars);
        
        if (bitIndex == 0)
        {
            EnsureAdditionalCapacity(byteCount);
            encoding.TryGetBytes(chars, buffer.AsSpan(byteIndex, byteCount), out int bytesWritten);
            Advance(bytesWritten);
        }
        else
        {
            var bytes = arrayPool.Rent(byteCount);
            encoding.TryGetBytes(chars, bytes, out int bytesWritten);
            WriteBytes(bytes.AsSpan(0, bytesWritten));
            arrayPool.Return(bytes);
        }
    }

    /// <summary>
    /// Writes a string of characters.
    /// </summary>
    /// <param name="str">The string to write.</param>
    /// <param name="encoding">The text encoding (<see cref="Encoding.UTF8"/> by default).</param>
    public void WriteString(string str, Encoding? encoding = null)
    {
        WriteChars(str, encoding);
    }

    /// <summary>
    /// Writes a serializable object.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="value">The object to write.</param>
    public void WriteObject<T>(T value) where T : IBitSerializable
    {
        value.Serialize(ref this);
    }

    /// <summary>
    /// Writes a blob of LZ4 compressed data.
    /// </summary>
    /// <param name="writeLength">Should the length of the compressed block written as a <see cref="uint"/> before the data.</param>
    /// <returns>The compressed data that can be written with <see cref="CompressedData.Writer"/>.</returns>
    [UnscopedRef]
    public CompressedData WriteLZ4(bool writeLength = false)
    {
        return new CompressedData(ref this, arrayPool, writeLength);
    }

    /// <summary>
    /// Releases the underlying buffer.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            arrayPool.Return(buffer);
        }
    }
}
