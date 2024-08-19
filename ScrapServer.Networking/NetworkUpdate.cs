using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;

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

        private Builder Write<TNetObj>(TNetObj netObj, NetworkUpdateType updateType) where TNetObj : INetObj
        {
            writer.GoToNearestByte();
            var sizePos = writer.ByteIndex;

            var header = new Networking.Data.NetObj
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

            switch(updateType)
            {
                case NetworkUpdateType.Create:
                    netObj.SerializeCreate(ref writer);
                    break;
                case NetworkUpdateType.P:
                    netObj.SerializeP(ref writer);
                    break;
                case NetworkUpdateType.Update:
                    netObj.SerializeUpdate(ref writer);
                    break;
                case NetworkUpdateType.Remove:
                    netObj.SerializeRemove(ref writer);
                    break;
                default:
                    throw new InvalidDataException($"Invalid update type: {updateType}");
            }

            Networking.Data.NetObj.WriteSize(ref writer, sizePos);

            return this;
        }

        public Builder WriteCreate<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Create);
        }

        public Builder WriteUpdate<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Update);
        }

        public Builder WriteP<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.P);
        }

        public Builder WriteRemove<TNetObj>(TNetObj obj) where TNetObj : INetObj
        {
            return Write(obj, NetworkUpdateType.Remove);
        }

        public NetworkUpdate Build()
        {
            var packet = new NetworkUpdate { GameTick = GameTick, Updates = writer.Data.ToArray() };
            writer.Dispose();

            return packet;
        }
    }
}
