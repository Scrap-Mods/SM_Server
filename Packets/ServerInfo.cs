using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;
using K4os.Compression.LZ4.Streams;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using static SMServer.Packets.ServerInfo;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public class BigEndianBinaryWriter : BinaryWriter
        {
            public BigEndianBinaryWriter(Stream output) : base(output) { }

            public override void Write(short value)
            {
                base.Write(IPAddress.HostToNetworkOrder(value));
            }

            public override void Write(int value)
            {
                base.Write(IPAddress.HostToNetworkOrder(value));
            }

            public override void Write(long value)
            {
                base.Write(IPAddress.HostToNetworkOrder(value));
            }

            public override void Write(ushort value)
            {
                base.Write((ushort)IPAddress.HostToNetworkOrder((short)value));
            }

            public override void Write(uint value)
            {
                base.Write((uint)IPAddress.HostToNetworkOrder((int)value));
            }

            public override void Write(ulong value)
            {
                base.Write((ulong)IPAddress.HostToNetworkOrder((long)value));
            }
        }

        public override byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BigEndianBinaryWriter(stream))
            {
                // Write the packet ID and all the data except Id
                //writer.Write(Id);
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

                // Convert the MemoryStream to a byte array
                byte[] uncompressedData = stream.ToArray();

                // Compress the data using lz4
                Span<byte> uncompressedDataSpan = new Span<byte>(uncompressedData);

                int maximumOutputSize = LZ4Codec.MaximumOutputSize(uncompressedData.Length);
                Span<byte> compressedDataSpan = new byte[maximumOutputSize];
                int compressedDataSize = LZ4Codec.Encode(
                    uncompressedDataSpan, compressedDataSpan, LZ4Level.L00_FAST);

                byte[] compressedData = compressedDataSpan.Slice(0, compressedDataSize).ToArray();

                // Prepend the packet ID to the compressed data
                byte[] packetData = new byte[compressedData.Length + 1];
                packetData[0] = Id;
                Array.Copy(compressedData, 0, packetData, 1, compressedData.Length);

                return packetData;
            }
        }


        protected override void Deserialize(BinaryReader reader)
        {
            //// Decompress the data using k40s LZ4
            //using (var lz4Stream = LZ4Stream.Decode(reader.BaseStream))
            //using (var decompressedStream = new MemoryStream())
            //{
            //    lz4Stream.CopyTo(decompressedStream);
            //    decompressedStream.Seek(1, SeekOrigin.Begin);

            //    using (var decompressedReader = new BinaryReader(decompressedStream))
            //    {
            //        // Read the deserialized data from the decompressed stream
            //        Version = decompressedReader.ReadUInt32();
            //        Gamemode = (EGamemode)decompressedReader.ReadUInt32();
            //        Seed = decompressedReader.ReadUInt32();
            //        GameTick = decompressedReader.ReadUInt32();

            //        var modDataCount = decompressedReader.ReadUInt32();
            //        MData = new ModData[modDataCount];
            //        for (var i = 0; i < modDataCount; i++)
            //        {
            //            MData[i] = new ModData();
            //            MData[i].Deserialize(decompressedReader);
            //        }

            //        var someDataCount = decompressedReader.ReadUInt32();
            //        SomeData = decompressedReader.ReadBytes((int)someDataCount);

            //        var scriptDataCount = decompressedReader.ReadUInt32();
            //        SData = new GenericData[scriptDataCount];
            //        for (var i = 0; i < scriptDataCount; i++)
            //        {
            //            SData[i] = new GenericData();
            //            SData[i].Deserialize(decompressedReader);
            //        }

            //        var genericDataCount = decompressedReader.ReadUInt32();
            //        GData = new GenericData[genericDataCount];
            //        for (var i = 0; i < genericDataCount; i++)
            //        {
            //            GData[i] = new GenericData();
            //            GData[i].Deserialize(decompressedReader);
            //        }

            //        Flags = decompressedReader.ReadByte();
            //    }
            //}
        }


    }
}
