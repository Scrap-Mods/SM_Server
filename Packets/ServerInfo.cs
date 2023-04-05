using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using K4os.Compression.LZ4.Streams;

namespace SMServer.Packets
{
    [Serializable]
    internal class ServerInfo : IPacket
    {
        public const byte PacketId = 2;

        public enum EGamemode
        {
            AlphaTerrain,
            FlatTerrain,
            ClassicTerrain,
            CreatedTerrain_Test,
            CreatedTerrain,
            Challenge,
            ChallengeBuilder,
            Terrain,
            MenuCreation,
            Survival = 14,
            Custom,
            Development
        }

        
        public class ModData
        {
            UInt64 FileId;
            Guid UUID;

            public void Serialize(BigEndianBinaryWriter writer)
            {
                writer.Write(FileId);
                writer.Write(UUID.ToByteArray());
            }

            public void Deserialize(BigEndianBinaryReader reader)
            {
                FileId = reader.ReadUInt64();
                UUID = new Guid(reader.ReadBytes(16));
            }
        }

        
        public class GenericData
        {
            Guid UUID;
            byte[] Key;

            public void Serialize(BigEndianBinaryWriter writer)
            {
                writer.Write(UUID.ToByteArray());
                writer.Write((UInt16)Key.Length);
                writer.Write(Key);
            }

            public void Deserialize(BigEndianBinaryReader reader)
            {
                UUID = new Guid(reader.ReadBytes(16));
                var keyLen = reader.ReadUInt16();
                Key = reader.ReadBytes(keyLen);
            }
        }

        public UInt32 Version;
        public EGamemode Gamemode;
        public UInt32 Seed;
        public UInt32 GameTick;
        public ModData[] MData;
        public byte[] SomeData;
        public GenericData[] SData;
        public GenericData[] GData;
        public byte Flags;

        public ServerInfo()
        {

        }

        // Constructor
        public ServerInfo(
            UInt32 version,
            EGamemode gamemode,
            UInt32 seed,
            UInt32 gameTick,
            ModData[] mData,
            byte[] someData,
            GenericData[] sData,
            GenericData[] gData,
            byte flags)
        {
            this.Version = version;
            this.Gamemode = gamemode;
            this.Seed = seed;
            this.GameTick = gameTick;
            this.MData = mData;
            this.SomeData = someData;
            this.SData = sData;
            this.GData = gData;
            this.Flags = flags;
        }

        public virtual void Serialize(BigEndianBinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((UInt32)Gamemode);
            writer.Write(Seed);
            writer.Write(GameTick);

            writer.Write((UInt32)MData.Length);
            foreach (var modData in MData)
            {
                modData.Serialize(writer);
            }

            writer.Write((UInt32)SomeData.Length);
            writer.Write(SomeData);

            writer.Write((UInt32)SData.Length);
            foreach (var scriptData in SData)
            {
                scriptData.Serialize(writer);
            }

            writer.Write((UInt32)GData.Length);
            foreach (var generictData in GData)
            {
                generictData.Serialize(writer);
            }

            writer.Write(Flags);
        }

        public virtual void Deserialize(BigEndianBinaryReader reader)
        {
            Version = reader.ReadUInt32();
            Gamemode = (EGamemode)reader.ReadUInt32();
            Seed = reader.ReadUInt32();
            GameTick = reader.ReadUInt32();

            var modDataCount = reader.ReadUInt32();
            MData = new ModData[modDataCount];
            for (var i = 0; i < modDataCount; i++)
            {
                MData[i] = new ModData();
                MData[i].Deserialize(reader);
            }

            var someDataCount = reader.ReadUInt32();
            SomeData = reader.ReadBytes((int)someDataCount);

            var scriptDataCount = reader.ReadUInt32();
            SData = new GenericData[scriptDataCount];
            for (var i = 0; i < scriptDataCount; i++)
            {
                SData[i] = new GenericData();
                SData[i].Deserialize(reader);
            }

            var genericDataCount = reader.ReadUInt32();
            GData = new GenericData[genericDataCount];
            for (var i = 0; i < genericDataCount; i++)
            {
                GData[i] = new GenericData();
                GData[i].Deserialize(reader);
            }

            Flags = reader.ReadByte();
        }
    }
}
