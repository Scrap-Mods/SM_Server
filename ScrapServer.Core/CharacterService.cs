using ScrapServer.Networking;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace ScrapServer.Core;

public struct Character
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 LookDirection;
}

public static class CharacterService
{
    public static Character[] Characters = [];

    public static void MoveCharacter(int characterId, byte moveDir, PlayerMovementKey key)
    {
        Vector3 vector = Vector3.Zero;

        if ((key & PlayerMovementKey.HORIZONTAL) == key) 
        {
            double angle = 2 * Math.PI * ((double)moveDir) / 255;
            vector = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0) * 2;
        }

        Characters[characterId].Velocity = vector;
    }

    //TODO(AP): Once networking is refactored, remove the "client" parameter
    public static void Tick(UInt32 tick, IClient client)
    {
        for (var i = 0; i < Characters.Length; i++)
        {
            var velocity = Characters[i].Velocity;

            Characters[i].Position += velocity;

            var stream = BitWriter.WithSharedPool();
            var netObj = new NetObjUnreliable { ObjectType = NetObjType.Character, Size = 0 };
            var updateCharacter = new UpdateUnreliableCharacter { CharacterId = i, IsTumbling = false };

            double angle = Math.Atan2(velocity.X, velocity.Y);
            byte binaryAngle = (byte)(255 * angle / (2 * Math.PI));

            var isNotTumbling = new IsNotTumbling
            {
                Horizontal = velocity == Vector3.Zero,
                Direction = binaryAngle,
                Yaw = 0,
                Pitch = 0
            };

            netObj.Serialize(ref stream);
            updateCharacter.Serialize(ref stream);
            isNotTumbling.Serialize(ref stream);
            NetObj.WriteSize(ref stream, 0);

            client.Send(new UnreliableUpdate { CurrentTick = tick, ServerTick = tick, Updates = stream.Data.ToArray() });
        }
    }
}