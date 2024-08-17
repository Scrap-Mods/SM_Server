using OpenTK.Mathematics;
using ScrapServer.Networking;
using ScrapServer.Networking.Data;
using ScrapServer.Utility.Serialization;
using ScrapServer.Core.NetObjs;
using System.Numerics;

namespace ScrapServer.Core;

public static class CharacterService
{
    public static Dictionary<Player, Character> Characters = [];

    private static byte ConvertAngleToBinary(double angle)
    {
        angle = (angle + Math.PI * 2) % (Math.PI * 2);

        return (byte)(256 * (angle) / (2 * Math.PI));
    }

    public static Character GetCharacter(Player player)
    {
        Character? character;
        var found = Characters.TryGetValue(player, out character);
        if (found) return character!;

        // If character isn't in Dict, we load it from the save database into the dict and return it

        // ...

        // If it still cannot be found, we make a new one
        character = Character.CreateBuilder()
            .WithOwnerId(player.SteamId)
            .WithPlayerId((byte)player.Id)
            .WithName(player.Username)
            .Build();

        Characters[player] = character;

        return character!;
    }

    public static void RemoveCharacter(Player player)
    {
        Character? character;
        _ = Characters.TryGetValue(player, out character);
        
        if (character is Character chara)
        {
            foreach (var player2 in PlayerService.GetPlayers())
            {
                chara.RemovePackets(player2, 0);
            }

            Characters.Remove(player);
        }
    }


    //TODO(AP): Move most of this logic to Character
    public static void Tick()
    {
        var stream = BitWriter.WithSharedPool();

        foreach (var character in Characters.Values)
        {
            stream.GoToNearestByte();
            var position = stream.ByteIndex;

            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };

            character.Velocity = 0.8f * character.Velocity + character.TargetVelocity * (1 - 0.8f);
            character.Position += character.Velocity * 0.025f * character.BodyRotation;

            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = character.Id, IsTumbling = false };

            double angleMove = Math.Atan2(character.Velocity.Y, character.Velocity.X);
            var anglesLook = character.HeadRotation.ExtractRotation().ToEulerAngles();

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = character.TargetVelocity != OpenTK.Mathematics.Vector3.Zero,
                SlowDown = character.IsSlowedDown,
                Crawl = character.IsCrouching,
                Sprint = character.IsSprinting,
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

        foreach (var client in PlayerService.GetPlayers())
        {
            client.Send(new UnreliableUpdate { CurrentTick = SchedulerService.GameTick, ServerTick = SchedulerService.GameTick, Updates = stream.Data.ToArray() });
        }

        stream.Dispose();
    }
}
