namespace ScrapServer.Networking.Client
{
    public struct ClientEventArgs
    {
        public IClient Client { get; }

        public ClientEventArgs(IClient client)
        {
            Client = client;
        }
    }
}
