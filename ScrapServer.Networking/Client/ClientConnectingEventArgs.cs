namespace ScrapServer.Networking.Client;

public class ClientConnectingEventArgs
{
    public IClient Client { get; }
    public bool IsAccepted { get; set; } = false;

    public ClientConnectingEventArgs(IClient client)
    {
        Client = client;
    }
}
