﻿using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking.Client;

/// <summary>
/// Represents a server listening for incoming connections.
/// </summary>
public interface IServer : IDisposable
{
    /// <summary>
    /// Gets the list of clients trying to establish a connection with this server.
    /// </summary>
    /// <value>List of connecting clients.</value>
    public IReadOnlyList<IClient> ConnectingClients { get; }

    /// <summary>
    /// Gets the list of clients connected to this server.
    /// </summary>
    /// <value>List of connected clients.</value>
    public IReadOnlyList<IClient> ConnectedClients { get; }

    /// <summary>
    /// Fired when a client is trying to connect to this server.
    /// Handlers should set call <see cref="IClient.AcceptConnection"/> 
    /// to accept the connection.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientConnecting;

    /// <summary>
    /// Fired when a client is connected to this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientConnected;

    /// <summary>
    /// Fired when a client is disconnected from this server.
    /// </summary>
    public event EventHandler<ClientEventArgs>? ClientDisconnected;

    /// <summary>
    /// Registers a handler for packets coming from any client 
    /// connected to this server.
    /// </summary>
    /// <param name="handler">The delegate to be called when a packet is received.</param>
    public void HandleRaw(RawPacketEventHandler handler);

    /// <summary>
    /// Registers a handler for packets with the specified id 
    /// coming from any client connected to this server.
    /// </summary>
    /// <param name="packetId">The id of packets handled by <paramref name="handler"/>.</param>
    /// <param name="handler">The delegate to be called when a matching packet is received.</param>
    public void HandleRaw(PacketId packetId, RawPacketEventHandler handler);

    /// <summary>
    /// Runs a single iteration of the event loop.
    /// </summary>
    public void Poll();

    /// <summary>
    /// Runs the server event loop.
    /// </summary>
    /// <remarks>
    /// Does not throw an <see cref="OperationCanceledException"/> 
    /// when <paramref name="cancellationToken"/> signals cancellation.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token for stopping the loop.</param>
    public void RunLoop(CancellationToken cancellationToken = default);
}
