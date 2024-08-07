using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Core;

public class Character
{
    public Matrix3 Rotation;
    public Vector3 Position = new Vector3(0, 0, 0.72f);
    public Vector3 Velocity;
}

public static class CharacterService
{
    private static byte ConvertAngleToBinary(double angle)
    {
       angle = (angle + Math.PI * 2) % (Math.PI * 2);

       return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public static List<Character> Characters = [];

    public static void MoveCharacter(int characterId, byte moveDir, PlayerMovementKey key)
    {
        Vector3 vector = Vector3.Zero;

        if ((key & PlayerMovementKey.HORIZONTAL) == PlayerMovementKey.HORIZONTAL) 
        {
            double direction = 2 * Math.PI * ((double)moveDir) / 256;
            vector = new Vector3((float)Math.Cos(direction) * 0.11f, (float)Math.Sin(direction) * 0.11f, 0);
        }

        Characters[characterId].Velocity = vector;
    }

    public static void LookCharacter(int characterId, byte yaw, byte pitch)
    {
        double angleYaw = 2 * Math.PI * ((double)yaw) / 256;
        double anglePitch = 2 * Math.PI * ((double)pitch) / 256;

        Characters[characterId].Rotation = Matrix3.CreateRotationZ((float)angleYaw);
    }

    //TODO(AP): Once networking is refactored, remove the "client" parameter
    public static void Tick(UInt32 tick, IClient client)
    {
        var stream = BitWriter.WithSharedPool();

        for (var i = 0; i < Characters.Count; i++)
        {
            stream.GoToNearestByte();
            var position = stream.ByteIndex;

            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };
            
            var velocity = Characters[i].Velocity;
            var rotation = Characters[i].Rotation;

            Characters[i].Position += velocity * rotation;

            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = i, IsTumbling = false };

            double angleMove = Math.Atan2(velocity.Y, velocity.X);
            var anglesLook = rotation.ExtractRotation().ToEulerAngles();

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = velocity != Vector3.Zero,
                Direction = ConvertAngleToBinary(angleMove),
                Yaw = ConvertAngleToBinary(anglesLook.Z),
                Pitch = ConvertAngleToBinary(anglesLook.X),
                Position = Characters[i].Position
            };

            netObj.Serialize(ref stream);
            updateCharacter.Serialize(ref stream);
            isNotTumbling.Serialize(ref stream);
            NetObjUnreliable.WriteSize(ref stream, position);
        }

        client.Send(new UnreliableUpdate { CurrentTick = tick, ServerTick = tick, Updates = stream.Data.ToArray() });
        stream.Dispose();
    }
}