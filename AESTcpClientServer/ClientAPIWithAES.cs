using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using WebAPI;
using WebAPI.SecureWebApi;

namespace TcpClientServerChat
{
    public class ClientAPIWithAES : WebApi_AES_ECDH
    {
        public class ClientAPIInitArgs
        {
            public string IP;
            public int Port;
            public string NickName;
        }

        public override string Title
        {
            get
            {
                if (IsWorking)
                    return $"{server.IP}:{server.Port} | {NickName}";
                return base.Title;
            }
        }

        private readonly TcpConnection server;
        private Thread UserReaderThread;

        private readonly ObservableCollection<string> _list_of_users;
        public override ReadOnlyCollection<string> ListOfUsers => _list_of_users.ToList().AsReadOnly();

        public override string NickName
        {
            get => $"{_nick}#{ID}";
            set
            {
                if (_nick != value)
                {
                    if (IsWorking)
                    {
                        SendMessage(new($"{_nick}#{ID}", $"Changed NickName to \"{value}\"", $"nick\x1{value}#{ID}"));
                    }
                    _nick = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }
        private int ID;

        public override void Start()
        {
            if (IsWorking) return;

            try { server.Connect(); }
            catch
            {
                AddNewMessage(new Message("Client", "Fail to connect", MessageService.Log));
                return;
            }

            var message = Message.FromBase64String(server.ReadMessage());
            AddNewMessage(message);
            if (message.Service == MessageService.NoPlaces)
            {
                server.Disconnect();
                return;
            }

            try {  aes.Key = KeyExchange(server); }
            catch (Exception e)
            {
                AddNewMessage(new Message("Client", e.Message, MessageService.Log));
                return;
            }

            SendMessage(new Message(_nick, string.Empty, $"nick\x1{_nick}"));

            UserReaderThread = new(new ThreadStart(ReadMessages)) { Name = "ClientAPI_ReadThread" };
            UserReaderThread.Start();
            IsWorking = true;
        }
        public override void Shutdown()
        {
            if (!IsWorking) return;
            IsWorking = false;

            SendMessage(new(NickName, "Disconnected from server", MessageService.Disconnect));
            server.Disconnect();

            _list_of_users.Clear();
        }

        public override void SendMessage(Message message)
        {
            if (message.Text != string.Empty && message.Service != MessageService.File)
                AddNewMessage(message);

            aes.GenerateIV();
            server.SendMessage(message.CreateSecureMessage(aes.Key, aes.IV));
        }

        private void ExecCMD(Message message)
        {
            if (message.Service == MessageService.Disconnect)
            {
                if (message.Sender == "Server")
                    server.Disconnect();
                else
                    _list_of_users.Remove(message.Sender);
            }
            else if (message.Service.Contains("max_file_size\x1"))
            {
                MaxFileSize = int.Parse(message.Service[14..]);
            }
            else if (message.Service == MessageService.File)
            {
                ReadFile(message);
            }
            else if (message.Service.Contains("nick\x1"))
            {
                var buf = message.Service.Split('\x1');
                _list_of_users[_list_of_users.IndexOf(message.Sender)] = message.Service[5..];
            }
            else if (message.Service.Contains("new_user\x1"))
            {
                _list_of_users.Add(message.Service[9..]);
            }
            else if (message.Service.Contains("id\x1"))
            {
                ID = int.Parse(message.Service[3..]);
                RaisePropertyChanged(nameof(Title));
            }
        }
        private void ReadMessages()
        {
            while (server?.Connected == true)
            {
                var base64_mesg = server.ReadMessage();
                if (base64_mesg == null) continue;

                var message = SecureMessageService.ReadSecureMessage(base64_mesg, aes.Key);
                if (message.Service != string.Empty)
                    ExecCMD(message);

                AddNewMessage(message);
            }
            Shutdown();
        }

        public ClientAPIWithAES(ClientAPIInitArgs new_connection_info) : base()
        {
            _list_of_users = new();
            _list_of_users.CollectionChanged += (s, e) => { RaisePropertyChanged(nameof(ListOfUsers)); };

            server = new(new_connection_info.IP, new_connection_info.Port);
            NickName = new_connection_info.NickName;
        }
    }
}
