namespace ScrapServer.Networking.Data;

[Flags]
public enum ServerFlags : byte
{
    None = 0,
    DeveloperMode = 0x80
}