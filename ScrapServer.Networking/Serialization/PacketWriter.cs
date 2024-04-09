using System.Buffers;

namespace ScrapServer.Networking.Serialization;

public ref struct PacketWriter
{
    private readonly ArrayPool<byte> arrayPool;

    private int index;
    private byte bitOffset;

    /// <summary>
    /// Initializes a new instance of <see cref="PacketWriter"/>.
    /// </summary>
    /// <param name="arrayPool">The array pool for renting buffers for writing.</param>
    internal PacketWriter(ArrayPool<byte> arrayPool)
    {
        this.arrayPool = arrayPool;
    }
}
