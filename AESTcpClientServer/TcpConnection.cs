using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TcpClientServerChat
{
    public class TcpConnection : WebAPI.IConnection
    {
        private readonly Socket socket;
        private NetworkStream socket_stream;
        private BinaryReader reader;
        private BinaryWriter writer;

        public bool Connected => socket.Connected;

        private string _ip;
        private int _port;
        public string IP
        {
            get => _ip;
            set
            {
                if (!Connected)
                {
                    _ip = value;
                }
            }
        }
        public int Port
        {
            get => _port;
            set
            {
                if (!Connected)
                {
                    _port = value;
                }
            }
        }

        private void StreamsInit()
        {
            socket_stream = new(socket);
            reader = new(socket_stream);
            writer = new(socket_stream);
        }

        public TcpConnection(Socket user_socket)
        {
            socket = user_socket;
            StreamsInit();
            if (user_socket.RemoteEndPoint != null)
            {
                _ip = (user_socket.RemoteEndPoint as IPEndPoint).Address.ToString();
                _port = (user_socket.RemoteEndPoint as IPEndPoint).Port;
            }
        }
        public TcpConnection(string ip, int port)
        {
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _ip = ip;
            _port = port;
        }

        public void SendMessage(string message)
        {
            if (Connected)
            {
                try
                {
                    writer.Write(message);
                }
                catch { }
            }
        }
        public string ReadMessage()
        {
            if (Connected)
            {
                try
                {
                    return reader.ReadString();
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public void Connect()
        {
            if (!Connected)
            {
                socket.Connect(new IPEndPoint(IPAddress.Parse(IP), Port));
                StreamsInit();
            }
        }

        public void Disconnect()
        {
            if (Connected)
            {
                socket.Close();
                socket_stream.Dispose();
                reader.Dispose();
                writer.Dispose();
            }
        }
    }
}
