﻿using Steamworks;
using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Client.Steam;
using ScrapServer.Networking.Packets.Data;
using ScrapServer.Utility.Serialization;

namespace ScrapServer;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            SteamClient.Init(387990);
            Console.WriteLine("Logged in as \"" + SteamClient.Name + "\" (" + SteamClient.SteamId + ")");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }

        string steamid = SteamClient.SteamId.ToString();
        const string server_passphrase = "balls";

        SteamFriends.SetRichPresence("status", "Hosting game");
        SteamFriends.SetRichPresence("passphrase", server_passphrase);
        SteamFriends.SetRichPresence("connect", string.Format("-connect_steam_id {0} -friend_steam_id {0}", steamid));

        var server = new SteamworksServer();

        server.ClientConnecting += (o, args) =>
        {
            Console.WriteLine($"Client connecting... {args.Client.Username}");
            args.Client.AcceptConnection();
        };

        server.ClientConnected += (o, args) =>
        {
            Console.WriteLine($"Client connected! {args.Client.Username}");

            args.Client.HandleRawPacket(PacketType.Hello, (o, args2) =>
            {
                Console.WriteLine("Received Hello");
                using var writer = BitWriter.FromSharedPool();
                var comp = writer.WriteLZ4();
                comp.Writer.WriteObject(new ServerInfo(
                    729, // protocol ver
                    Gamemode.FlatTerrain,
                    397817921, // seed
                    0, // game tick
                    new ModData[0],
                    new byte[0],
                    new GenericData[0],
                    new GenericData[0],
                    ServerFlags.None // flags)
                ));
                comp.Dispose();
                args.Client.SendRawPacket(PacketType.ServerInfo, writer.Data);
                Console.WriteLine("Sent ServerInfo");
            });

            args.Client.SendRawPacket(PacketType.ClientAccepted, Array.Empty<byte>());
            Console.WriteLine("Sent ClientAccepted");
        };

        while (true)
        {
            server.Poll();

            // if user pressed DEL in console, close the server:
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Delete)
            {
                Console.WriteLine("exiting...");
                break;
            }
        }
        server.Dispose();
        SteamClient.Shutdown();
    }
}
