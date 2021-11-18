namespace WebAPI
{
    public interface IConnection
    {
        bool Connected { get; }

        void SendMessage(string message);
        string ReadMessage();

        void Connect();
        void Disconnect();
    }
}
