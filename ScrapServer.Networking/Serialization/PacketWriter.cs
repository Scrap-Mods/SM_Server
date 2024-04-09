using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ScrapServer.Networking.Serialization;

/// <summary>
/// Writes non bit-aligned binary data to an array.
/// </summary>
public struct PacketWriter
{
    /// <summary>
    /// Gets the written data.
    /// </summary>
    /// <value>The span containing the written data.</value>
    public readonly ReadOnlySpan<byte> Data => new ReadOnlySpan<byte>(buffer, 0, index + (bitIndex != 0 ? 1 : 0));

    private readonly ArrayPool<byte> arrayPool;
    private byte[] buffer;

    private int index;
    private int bitIndex;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketWriter"/>.
    /// </summary>
    /// <param name="arrayPool">The array pool for renting buffers for writing.</param>
    internal PacketWriter(ArrayPool<byte> arrayPool)
    {
        this.arrayPool = arrayPool;
        buffer = Array.Empty<byte>();
    }

    /// <summary>
    /// Preallocates the specified number of bits and bytes.
    /// </summary>
    /// <param name="byteCount">The desired number of bytes.</param>
    /// <param name="bitCount">The desired number of bits.</param>
    public void EnsureAdditionalCapacity(int byteCount, int bitCount = 0)
    {
        int oldLength = index + bitIndex / 8 + (bitIndex % 8 != 0 ? 1 : 0);
        int newLength = index + byteCount + (bitIndex + bitCount) / 8 + ((bitIndex + bitCount) % 8 != 0 ? 1 : 0);
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

    private void Advance(int byteCount, int bitCount = 0)
    {
        index = index + byteCount + (bitIndex + bitCount) / 8;
        bitIndex = (bitIndex + bitCount) % 8;
    }

    /// <summary>
    /// Fills the specified number of bits and bytes with zeroes.
    /// </summary>
    /// <param name="byteCount">The number of bytes.</param>
    /// <param name="bitCount">The number of bits.</param>
    public void WritePadding(int byteCount, int bitCount = 0)
    {
        EnsureAdditionalCapacity(byteCount, bitCount);
        Advance(byteCount, bitCount);
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
            buffer[index] = unchecked((byte)(buffer[index] | BitHelper.Bit(bitIndex)));
        }
        Advance(0, 1);
    }

    /// <summary>
    /// Writes a <see cref="byte"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteUInt8(byte value)
    {
        EnsureAdditionalCapacity(1);
        if (bitIndex == 0)
        {
            buffer[index] = value;
        }
        else
        {
            buffer[index] = unchecked((byte)(buffer[index] | (value >>> bitIndex)));
            buffer[index + 1] = unchecked((byte)(value << (8 - bitIndex)));
        }
        Advance(1);
    }

    /// <summary>
    /// Writes a <see cref="sbyte"/>.
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteInt8(sbyte value)
    {
        WriteUInt8(unchecked((byte)value));
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
            bytes.CopyTo(buffer.AsSpan(index, bytes.Length));
        }
        else
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                WriteUInt8(bytes[i]);
            }
        }
        Advance(bytes.Length);
    }

    /// <summary>
    /// Writes a boolean value as a byte (<c>0x01</c> if true, <c>0x00</c> if false).
    /// </summary>
    /// <param name="value">The value to write.</param>
    public void WriteBoolean(bool value)
    {
        WriteUInt8((byte)(value ? 1 : 0));
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
    public void WriteGuid(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        value.TryWriteBytes(bytes);
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
        EnsureAdditionalCapacity(byteCount);

        if (bitIndex == 0)
        {
            encoding.TryGetBytes(chars, buffer.AsSpan(index, byteCount), out int bytesWritten);
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
    public void WriteObject<T>(T value) where T : INetworkObject
    {
        value.Serialize(ref this);
    }

    /// <summary>
    /// Writes a blob of LZ4 compressed data.
    /// </summary>
    /// <returns>The compressed data that can be written with <see cref="CompressedData.Writer"/>.</returns>
    [UnscopedRef]
    public CompressedData WriteLZ4()
    {
        return new CompressedData(ref this, arrayPool);
    }
}
