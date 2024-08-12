
using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking;

public struct CompoundPacket : IPacket
{
    public ref struct Builder
    {
        private BitWriter writer;

        public Builder()
        {
            writer = BitWriter.WithSharedPool();
        }
            
        public Builder Write<T>(T data) where T : IPacket
        {
            writer.GoToNearestByte();
            var byteIndexBefore = writer.ByteIndex;

            writer.WriteUInt32(0);
            writer.WriteByte((byte)T.PacketId);
            writer.WriteObject(data);

            var byteIndexAfter = writer.ByteIndex;
            var bitIndexAfter = writer.BitIndex;

            writer.Seek(byteIndexBefore);

            writer.WriteUInt32((UInt32) (byteIndexAfter - byteIndexBefore - 4));

            writer.Seek(byteIndexAfter, bitIndexAfter);

            return this;
        }

        public CompoundPacket Build()
        {
            var packet = new CompoundPacket { Data = writer.Data.ToArray() };
            writer.Dispose();

            return packet;
        }
    }

    public byte[]? Data;

    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.CompoundPacket;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public void Deserialize(ref BitReader reader)
    {
        Data = new byte[reader.BytesLeft];
        reader.ReadBytes(Data);
    }

    public void Serialize(ref BitWriter writer)
    {
        if (Data != null)
        {
            writer.WriteBytes(Data);
        }
    }
}
