using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Core;

public class Character
{
    public Matrix3 HeadRotation = Matrix3.Identity;
    public Matrix3 BodyRotation = Matrix3.Identity;
    public Vector3 Position = new Vector3(0, 0, 0.72f);
    public Vector3 Velocity = Vector3.Zero;
}

public static class CharacterService
{
    private static Dictionary<int, Character> Characters = [];
    private static int GlobalCharacterCount = 0;

    private static byte ConvertAngleToBinary(double angle)
    {
        angle = (angle + Math.PI * 2) % (Math.PI * 2);

        return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public static int SpawnCharacter(Character character)
    {
        Characters[GlobalCharacterCount++] = character;

        return GlobalCharacterCount;
    }

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

        var matrixYaw = Matrix3.CreateRotationZ((float)angleYaw);
        var matrixPitch = Matrix3.CreateRotationX((float)anglePitch);

        Characters[characterId].BodyRotation = matrixYaw;
        Characters[characterId].HeadRotation = matrixYaw * matrixPitch;
    }

    //TODO(AP): Once networking is refactored, remove the "client" parameter
    public static void Tick(UInt32 tick, IClient client)
    {
        var stream = BitWriter.WithSharedPool();

        foreach (var character in Characters.Values)
        {
            stream.GoToNearestByte();
            var position = stream.ByteIndex;

            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };

            character.Position += character.Velocity * character.BodyRotation;

            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = i, IsTumbling = false };

            double angleMove = Math.Atan2(character.Velocity.Y, character.Velocity.X);
            var anglesLook = character.HeadRotation.ExtractRotation().ToEulerAngles();

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = character.Velocity != Vector3.Zero,
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