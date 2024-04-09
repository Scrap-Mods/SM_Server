using ScrapServer.Networking.Packets.Data;
using ScrapServer.Networking.Packets.Utils;
using ScrapServer.Utility.Serialization;

namespace ScrapServer.Networking.Packets;

public class Character : IPacket
{
    public static PacketType PacketId => PacketType.CharacterInfo;

    //typedef struct {
    //    byte data[16];
    //}uuid;

    //typedef struct {
    //    char magic[2];
    //    byte Hair; // A3 = leftmost hair, A7 = rightmost hair
    //    byte name_len;
    //    char name[name_len]; // TODO: Check how name affects packet
    //    char magic_char[4]; // Check if magic is 00 00 00 02
    //    byte is_male;
    //    byte num_uuids;
    //    uuid uuids[num_uuids];
    //    BigEndian();
    //    uint32 skin_id; // 0-4
    //    LittleEndian();
    //    char magic2[11];
    //}
    //SMPacket9 < optimize = false >;

    // Constructor
    public Character()
    {

    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WritePacketType(PacketId);
    }

    public void Deserialize(ref BitReader reader)
    {
        reader.ReadPacketType();
        // packet has no additional data to deserialize
    }
}
