using ScrapServer.Networking.Packets;
using ScrapServer.Networking.Packets.Data;

namespace ScrapServer.Networking.Client;

/// <summary>
/// A delegate for handling incoming messages from the client.
/// </summary>
/// <param name="sender">The sender of the event.</param>
/// <param name="args">The event args.</param>
public delegate void PacketEventHandler(object? sender, RawPacketEventArgs args);

/// <summary>
/// Represents a client connected to a <see cref="IServer"/> for sending and receiving packets.
/// </summary>
public interface IClient : IDisposable
{
    /// <summary>
    /// Gets the username of the client.
    /// </summary>
    /// <value>The username or <see langword="null"/> if unknown.</value>
    public string? Username { get; }

    /// <summary>
    /// Gets the current state of the client.
    /// </summary>
    /// <value>Client state.</value>
    public ClientState State { get; }

    /// <summary>
    /// Fired when the state of client is changed.
    /// </summary>
    public event EventHandler<ClientEventArgs>? StateChanged;

    /// <summary>
    /// Registers a handler for packets with the specified id coming from the client.
    /// </summary>
    /// <param name="packetId">The id of packets handled by <paramref name="handler"/>.</param>
    /// <param name="handler">The delegate to be called when a matching packet is received.</param>
    public void HandleRawPacket(PacketId packetId, PacketEventHandler handler);

    /// <summary>
    /// Sends a packet to the client.
    /// </summary>
    /// <remarks>
    /// <paramref name="data"/> should be already compressed if needed
    /// and should not contain <paramref name="packetId"/>.
    /// </remarks>
    /// <param name="packetId">The packet id.</param>
    /// <param name="data">The data of the packet.</param>
    public void SendRawPacket(PacketId packetId, ReadOnlySpan<byte> data);

    /// <summary>
    /// Accepts the incoming connection.
    /// </summary>
    /// <remarks>
    /// Does nothing when <see cref="State"/> has any value other than <see cref="ClientState.Connecting"/>.
    /// </remarks>
    public void AcceptConnection();

    /// <summary>
    /// Disconnects the client from the server.
    /// </summary>
    public void Disconnect();

#pragma warning disable CA1816 // GC.SuppressFinalize is intended to be called in Disconnect.
    void IDisposable.Dispose() => Disconnect();
#pragma warning restore CA1816
}
