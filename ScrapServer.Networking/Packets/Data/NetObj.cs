using ScrapServer.Utility.Serialization;
using Steamworks;

namespace ScrapServer.Networking.Packets.Data;

public enum ControllerType
{
    ElectricMotor = 0x1,
    MotorController = 0x2,
    SteeringController = 0x3,
    SeatController = 0x4,
    SequenceController = 0x5,
    ButtonController = 0x6,
    LeverController = 0x7,
    SensorController = 0x8,
    ThrusterController = 0x9,
    RadioController = 0xA,
    HornController = 0xB,
    ToneController = 0xC,
    LogicController = 0xD,
    TimerController = 0xE,
    ParticlePreviewController = 0xF,
    SpringController = 0x10,
    SpotLightController = 0x11,
    PointLightController = 0x12,
    ChestController = 0x13,
    ItemStackController = 0x14,
    ScriptController = 0x15,
    PistonController = 0x16,
    SimpleInteractableController = 0x17,
    CameraController = 0x18,
    SurvivalThrusterController = 0x1A,
    SurvivalPistonController = 0x1B,
    SurvivalSpringController = 0x1C,
    SurvivalSequenceController = 0x1D,
    SurvivalSensorController = 0x1E,
};

public enum NetworkUpdateType {
    Create = 0x1,
    P = 0x2,
    Update = 0x3,
    Error = 0x4,
    Remove = 0x5
};

public enum NetObjType {
    RigidBody = 0,
    ChildShape = 1,
    Joint = 2,
    Controller = 3,
    Container = 4,
    Harvestable = 5,
    Character = 6,
    Lift = 7,
    Tool = 8,
    Portal = 9,
    PathNode = 10,
    Unit = 11,
    VoxelTerrainCell = 12,
    ScriptableObject = 13,
    ShapeGroup = 14
};

public struct NetObj : IBitSerializable
{
    public UInt16 Size;
    public NetworkUpdateType UpdateType;
    public NetObjType ObjectType;

    public void Deserialize(ref BitReader reader)
    {
        Size = reader.ReadUInt16();
        byte combined = reader.ReadByte();

        UpdateType = (NetworkUpdateType) (combined >> 5);
        ObjectType = (NetObjType) (combined & 0b00011111);
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt16(Size);
        byte combined = (byte)((byte)ObjectType | (byte)UpdateType << 5);

        writer.WriteByte(combined);
    }

    public static void WriteSize(ref BitWriter writer, int position)
    {
        int byteIndex = writer.ByteIndex;
        int bitIndex = writer.BitIndex;

        writer.Seek(position);

        UInt16 size = (UInt16)(byteIndex - position);

        writer.WriteUInt16(size);
        writer.Seek(byteIndex, bitIndex);
    }
}

public struct NetObjUnreliable : IBitSerializable
{
    public UInt16 Size;
    public NetObjType ObjectType;

    public void Deserialize(ref BitReader reader)
    {
        Size = reader.ReadUInt16();
        ObjectType = (NetObjType) reader.ReadByte();
    }

    public void Serialize(ref BitWriter writer)
    {
        writer.WriteUInt16(Size);
        writer.WriteByte((byte)ObjectType);
    }

    public static void WriteSize(ref BitWriter writer, int position)
    {
        int byteIndex = writer.ByteIndex;
        int bitIndex = writer.BitIndex;

        writer.Seek(position);

        UInt16 size = (UInt16)(byteIndex - position);

        writer.WriteUInt16(size);
        writer.Seek(byteIndex, bitIndex);
    }
}