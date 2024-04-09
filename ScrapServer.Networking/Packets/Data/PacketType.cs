namespace ScrapServer.Networking.Packets.Data;

public enum PacketType : byte
{
    Empty = 0,
    Hello = 1,
    ServerInfo = 2,
    RequestPassphrase = 3,
    RespondPassphrase = 4,
    ClientAccepted = 5,
    FileChecksums = 6,
    ChecksumsAccepted = 7,
    ChecksumsDenied = 8,
    CharacterInfo = 9,
    JoinConfirmation = 10,
    ScriptInitData = 11,
    GenericInitData = 13,
    DisplayMessage = 18,
    DisplayAlertText = 19
}
