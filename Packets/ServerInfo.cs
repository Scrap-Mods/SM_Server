using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

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

        public override byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Id);
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

                // Compress the written data using k40s LZ4
                using (var lz4Stream = LZ4Stream.Encode(stream, LZ4Level.L00_FAST))
                {
                    lz4Stream.Flush();
                }

                return stream.ToArray();
            }
        }



        protected override void Deserialize(BinaryReader reader)
        {
            // Decompress the data using k40s LZ4
            using (var lz4Stream = LZ4Stream.Decode(reader.BaseStream))
            using (var decompressedStream = new MemoryStream())
            {
                lz4Stream.CopyTo(decompressedStream);
                decompressedStream.Seek(0, SeekOrigin.Begin);

                using (var decompressedReader = new BinaryReader(decompressedStream))
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
}
