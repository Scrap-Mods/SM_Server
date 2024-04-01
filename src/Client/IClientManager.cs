namespace SMServer.src.Client;

public interface IClientManager
{
    public IReadOnlyList<IClient> ConnectedClients { get; }

    public event EventHandler<ClientConnectingEventArgs>? ClientConnecting;
    public event EventHandler<ClientEventArgs>? ClientConnected;
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

    public void RunLoop(CancellationToken cancellationToken = default);
}
