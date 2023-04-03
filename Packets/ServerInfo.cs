using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using K4os.Compression.LZ4.Streams;

namespace SMServer.Packets
{
    internal class ServerInfo : IPacket
    {
        public static readonly byte Id = 2;

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

            public void Serialize(BinaryWriter writer)
            {
                writer.Write(FileId);
                writer.Write(UUID.ToByteArray());
            }

            public void Deserialize(BinaryReader reader)
            {
                FileId = reader.ReadUInt64();
                UUID = new Guid(reader.ReadBytes(16));
            }
        }

        
        public class GenericData
        {
            Guid UUID;
            byte[] Key;

            public void Serialize(BinaryWriter writer)
            {
                writer.Write(UUID.ToByteArray());
                writer.Write((UInt16)Key.Length);
                writer.Write(Key);
            }

            public void Deserialize(BinaryReader reader)
            {
                UUID = new Guid(reader.ReadBytes(16));
                var keyLen = reader.ReadUInt16();
                Key = reader.ReadBytes(keyLen);
            }
        }

        UInt32 Version;
        EGamemode Gamemode;
        UInt32 Seed;
        UInt32 GameTick;
        ModData[] MData;
        byte[] SomeData;
        GenericData[] SData;
        GenericData[] GData;
        byte Flags;

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

        public override void Serialize(ref BigEndianBinaryWriter writer)
        {
            base.Serialize(ref writer);

            using (var stream = new MemoryStream())
            using (var writer2 = new BigEndianBinaryWriter(stream))
            {
                writer2.Write(Version);
                writer2.Write((UInt32)Gamemode);
                writer2.Write(Seed);
                writer2.Write(GameTick);

                writer2.Write((UInt32)MData.Length);
                foreach (var modData in MData)
                {
                    modData.Serialize(writer2);
                }

                writer2.Write((UInt32)SomeData.Length);
                writer2.Write(SomeData);

                writer2.Write((UInt32)SData.Length);
                foreach (var scriptData in SData)
                {
                    scriptData.Serialize(writer2);
                }

                writer2.Write((UInt32)GData.Length);
                foreach (var generictData in GData)
                {
                    generictData.Serialize(writer2);
                }

                writer2.Write(Flags);

                // Convert the MemoryStream to a byte array
                byte[] compressedData = LZ4.Compress(stream.ToArray());
                writer.Write(compressedData);
            }
        }


        protected override void Deserialize(BinaryReader reader)
        {
            // Read the compressed data from the stream
            byte[] compressedData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

            // Skip the packet ID at the beginning of the compressed data
            byte[] uncompressedData = new byte[compressedData.Length - 1];
            Array.Copy(compressedData, 1, uncompressedData, 0, uncompressedData.Length);

            // Decompress the data using K4os LZ4
            int uncompressedSize = (int)BitConverter.ToUInt32(compressedData, 1);
            byte[] decompressedData = new byte[uncompressedSize];
            LZ4Codec.Decode(uncompressedData, decompressedData);

            // Deserialize the decompressed data
            using (var decompressedStream = new MemoryStream(decompressedData))
            using (var decompressedReader = new BigEndianBinaryReader(decompressedStream))
            {
                // Read the deserialized data from the decompressed stream
                Version = decompressedReader.ReadUInt32();
                Gamemode = (EGamemode)decompressedReader.ReadUInt32();
                Seed = decompressedReader.ReadUInt32();
                GameTick = decompressedReader.ReadUInt32();

                var modDataCount = decompressedReader.ReadUInt32();
                MData = new ModData[modDataCount];
                for (var i = 0; i < modDataCount; i++)
                {
                    MData[i] = new ModData();
                    MData[i].Deserialize(decompressedReader);
                }

                var someDataCount = decompressedReader.ReadUInt32();
                SomeData = decompressedReader.ReadBytes((int)someDataCount);

                var scriptDataCount = decompressedReader.ReadUInt32();
                SData = new GenericData[scriptDataCount];
                for (var i = 0; i < scriptDataCount; i++)
                {
                    SData[i] = new GenericData();
                    SData[i].Deserialize(decompressedReader);
                }

                var genericDataCount = decompressedReader.ReadUInt32();
                GData = new GenericData[genericDataCount];
                for (var i = 0; i < genericDataCount; i++)
                {
                    GData[i] = new GenericData();
                    GData[i].Deserialize(decompressedReader);
                }

                Flags = decompressedReader.ReadByte();
            }
        }



    }
}
