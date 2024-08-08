using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using OpenTK.Mathematics;

namespace ScrapServer.Core;

public class Character
{
    public int Id;
    public Matrix3 HeadRotation = Matrix3.Identity;
    public Matrix3 BodyRotation = Matrix3.Identity;
    public Vector3 Position = new Vector3(0, 0, 0.72f);
    public Vector3 Velocity = Vector3.Zero;
}

public static class CharacterService
{
    private static Dictionary<Player, Character> Characters = [];
    private static int NextCharacterID = 2;

    private static byte ConvertAngleToBinary(double angle)
    {
        angle = (angle + Math.PI * 2) % (Math.PI * 2);

        return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public static Character GetCharacter(Player player)
    {
        Character? character;
        var found = Characters.TryGetValue(player, out character);

        if (found) return character;

        // If character isn't in Dict, we load it from the save database into the dict and return it

        // ...

        // If it still cannot be found, we make a new one
        character = new Character
        {
            Id = NextCharacterID
        };

        Characters[player] = character;
        NextCharacterID += 1;

        return character;
    }

    public static void MoveCharacter(Player player, byte moveDir, PlayerMovementKey key)
    {
        Vector3 vector = Vector3.Zero;

        if ((key & PlayerMovementKey.HORIZONTAL) == PlayerMovementKey.HORIZONTAL)
        {
            double direction = 2 * Math.PI * ((double)moveDir) / 256;
            vector = new Vector3((float)Math.Cos(direction) * 0.11f, (float)Math.Sin(direction) * 0.11f, 0);
        }

        Characters[player].Velocity = vector;
    }

    public static void LookCharacter(Player playerId, byte yaw, byte pitch)
    {
        double angleYaw = 2 * Math.PI * ((double)yaw) / 256;
        double anglePitch = 2 * Math.PI * ((double)pitch) / 256;

        var matrixYaw = Matrix3.CreateRotationZ((float)angleYaw);
        var matrixPitch = Matrix3.CreateRotationX((float)anglePitch);

        Characters[playerId].BodyRotation = matrixYaw;
        Characters[playerId].HeadRotation = matrixYaw * matrixPitch;
    }

    //TODO(AP): Once networking is refactored, remove the "client" parameter
    public static void Tick(UInt32 tick)
    {
        var stream = BitWriter.WithSharedPool();

        foreach (var character in Characters.Values)
        {
            stream.GoToNearestByte();
            var position = stream.ByteIndex;

            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };

            character.Position += character.Velocity * character.BodyRotation;

            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = character.Id, IsTumbling = false };

            double angleMove = Math.Atan2(character.Velocity.Y, character.Velocity.X);
            var anglesLook = character.HeadRotation.ExtractRotation().ToEulerAngles();

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = character.Velocity != Vector3.Zero,
                Direction = ConvertAngleToBinary(angleMove),
                Yaw = ConvertAngleToBinary(anglesLook.Z),
                Pitch = ConvertAngleToBinary(anglesLook.X),
                Position = character.Position
            };

            netObj.Serialize(ref stream);
            updateCharacter.Serialize(ref stream);
            isNotTumbling.Serialize(ref stream);
            NetObjUnreliable.WriteSize(ref stream, position);
        }
        
        foreach (var client in PlayerService.Players.Keys)
        {
            client.Send(new UnreliableUpdate { CurrentTick = tick, ServerTick = tick, Updates = stream.Data.ToArray() });
        }

        stream.Dispose();
    }
}