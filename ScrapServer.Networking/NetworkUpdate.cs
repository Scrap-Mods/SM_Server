using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;
using static ScrapServer.Networking.NetworkUpdate.Builder;

namespace ScrapServer.Networking;

public struct NetworkUpdate : IPacket
{
    /// <inheritdoc/>
    public static PacketId PacketId => PacketId.NetworkUpdate;

    /// <inheritdoc/>
    public static bool IsCompressable => true;

    public UInt32 GameTick;
    public byte[] Updates;

    public void Deserialize(ref BitReader reader)
    {
        GameTick = reader.ReadUInt32();
        Updates = new byte[reader.BytesLeft];
        reader.ReadBytes(Updates);
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt32(GameTick);
        writer.WriteBytes(Updates);
    }

    public ref struct Builder
    {
        private BitWriter writer;
        private UInt32 GameTick;

        public Builder()
        {
            writer = BitWriter.WithSharedPool();
        }

        public Builder WithGameTick(UInt32 gameTick)
        {
            GameTick = gameTick;
            return this;
        }

        public delegate void WriteDelegate(ref BitWriter writer);

        public Builder Write<TNetObj>(TNetObj netObj, NetworkUpdateType updateType, WriteDelegate writeDelegate) where TNetObj : INetObj
        {
            writer.GoToNearestByte();
            var sizePos = writer.ByteIndex;

            var header = new NetObj
            {
                UpdateType = updateType,
                ObjectType = netObj.NetObjType,
            };
            header.Serialize(ref writer);

            // The controller type is only sent on create
            if (updateType == NetworkUpdateType.Create)
            {
                writer.WriteByte((byte)netObj.ControllerType);
            }

            writer.WriteUInt32(netObj.Id);

            writeDelegate.Invoke(ref writer);

            NetObj.WriteSize(ref writer, sizePos);

            return this;
        }

        public Builder WriteCreate<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Create, obj.SerializeCreate);
        }

        public Builder WriteUpdate<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Update, obj.SerializeUpdate);
        }

        public Builder WriteP<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.P, obj.SerializeP);
        }

        public Builder WriteRemove<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Remove, obj.SerializeRemove);
        }

        public NetworkUpdate Build()
        {
            var packet = new NetworkUpdate { GameTick = GameTick, Updates = writer.Data.ToArray() };
            writer.Dispose();

            return packet;
        }
    }
}
