using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WebAPI;

namespace TcpClientServerChat
{
    public class ServerAPI : SecureWebAPI
    {
        public override string Title
        {
            get
            {
                if (IsWorking)
                    return $"{localIP} | {publicIP} | {Port} | {CurrentCountOfUsers}/{MaxCountOfUsers} | {NickName}";
                return base.Title;
            }
        }

        private readonly Random rnd = new();

        class User
        {
            public Connection Connection = null;
            public Thread ReaderThread = null;

            public string FullName => $"{NickName}#{ID}";
            public string NickName = null;
            public int ID = 0;

            public byte[] Key = null;
        }
        public class ServerAPIInitArgs
        {
            public int Port;
            public int MaxFileSize;
            public int MaxUsersCount;
            public string NickName;
        }

        private readonly Socket ServerSocket;
        public readonly string localIP, publicIP;
        public readonly int Port;
        private Thread ListeningThread;
        public override string NickName
        {
            get => $"{_nick}#0";
            set
            {
                if (_nick != value)
                {
                    if (IsWorking)
                    {
                        SendMessage(new($"{_nick}#0", $"Changed NickName to \"{value}\"", $"nick\x1{value}#0"));
                    }
                    _nick = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        private readonly object _list_of_users_locker = 0;
        private readonly ObservableCollection<User> _list_of_users;
        public override ReadOnlyCollection<string> ListOfUsers => _list_of_users.Select(x => x.FullName).ToList().AsReadOnly();
        public int CurrentCountOfUsers => _list_of_users.Count;
        private int _max_count_of_users;
        public int MaxCountOfUsers
        {
            get => _max_count_of_users;
            set
            {
                int new_value = value == 0 ? int.MaxValue : value;
                if (_max_count_of_users == new_value) return;

                _max_count_of_users = new_value;

                if (!IsWorking) return;

                SendMessage(new Message("Server", $"Max count of users changed to: {new_value}", MessageService.Log));
                if (CurrentCountOfUsers > new_value)
                {
                    lock (_list_of_users_locker)
                    {
                        while (CurrentCountOfUsers != new_value)
                            DisconnectAndRemoveUser(_list_of_users[new_value]);
                    }
                }
                RaisePropertyChanged(nameof(Title));
            }
        }
        public override int MaxFileSize
        {
            get => _max_file_size;
            set
            {
                int new_value = value == 0 ? int.MaxValue : value;
                if (_max_file_size == new_value) return;

                _max_file_size = new_value;
                if (IsWorking)
                {
                    SendMessage(new(
                        "Server",
                        $"Max size of file changed to: {_max_file_size} MiB",
                        MessageService.MaxFileSize(_max_file_size)));
                }

            }
        }
        private void SendMessageTo(Message message, User user)
        {
            if (user.Key != null)
            {
                aes.GenerateIV();
                user.Connection.SendMessage(message.CreateSecureMessage(user.Key, aes.IV));
            }
            else
            {
                user.Connection.SendMessage(message.ToBase64String());
            }
        }
        private void SendMessageToAllExcept(Message message, User user)
        {
            if (message.Service != MessageService.File)
                AddNewMessage(message);
            lock (_list_of_users_locker)
            {
                aes.GenerateIV();
                foreach (var u in _list_of_users.Where(u => u != user))
                    u.Connection.SendMessage(message.CreateSecureMessage(u.Key, aes.IV));
            }
        }
        public override void SendMessage(Message message) => SendMessageToAllExcept(message, null);
        public override void SendText(string text) => SendMessage(new(NickName, text, string.Empty));

        private void DisconnectAndRemoveUser(User user)
        {
            bool res;
            lock (_list_of_users_locker)
            {
                res = _list_of_users.Remove(user);
                RaisePropertyChanged(nameof(Title));
            }
            if (res)
            {
                user.Connection.Disconnect();
                SendMessage(new(user.FullName, "Disconnected from server", MessageService.Disconnect));
            }
        }
        private void UserChangedNickName(User user, string newNickName)
        {
            int index = newNickName.LastIndexOf('#');
            user.NickName = newNickName[0..index];
            RaisePropertyChanged(nameof(ListOfUsers));
        }
        private void ExecCMD(User sender_of_cmd, Message message)
        {
            if (message.Service == MessageService.Disconnect)
                DisconnectAndRemoveUser(sender_of_cmd);
            else if (message.Service.Contains("nick\x1"))
                UserChangedNickName(sender_of_cmd, message.Service[5..]);
            else if (message.Service == MessageService.File)
            {
                ReadFile(message);
                AddNewMessage(message);
            }
        }
        public void ReadMessageFrom(object user_obj)
        {
            var user = user_obj as User;
            while (user.Connection.Connected == true)
            {
                var base64_message = user.Connection.ReadMessage();
                if (base64_message == null) continue;

                Message message = SecureMessageService.ReadSecureMessage(base64_message, user.Key);
                if (message.Service != MessageService.Disconnect)
                    SendMessageToAllExcept(message, user);
                if (message.Service != string.Empty)
                    ExecCMD(user, message);
            }
            DisconnectAndRemoveUser(user);
        }

        private void SendHistoryToNewUser(User newUser)
        {
            lock (_list_of_messages_locker)
            {
                var valid_messages = ListOfMessages
                                     .Where(x => x.Text != string.Empty &&
                                            !x.Service.Contains("new_user") &&
                                            !x.Service.Contains("nick"))
                                     .Skip(Math.Max(0, ListOfMessages.Count - 50)).ToArray();
                foreach (Message m in valid_messages)
                {
                    if (m.Service == MessageService.File)
                    {
                        string filepath = m.OtherData;

                        try { CheckFile(filepath, MaxFileSize); }
                        catch
                        {
                            _list_of_messages.Remove(m);
                            continue;
                        }

                        m.OtherData = Convert.ToBase64String(File.ReadAllBytes(filepath));
                        SendMessageTo(m, newUser);
                        m.OtherData = filepath;
                        GC.Collect();
                    }
                    else
                        SendMessageTo(m, newUser);
                }
            }
        }
        private void AcceptNewUser(User newUser)
        {
            {
                newUser.Key = KeyExchange(newUser.Connection);
                var nick_message = SecureMessageService.ReadSecureMessage(newUser.Connection.ReadMessage(), newUser.Key);
                if (nick_message == null)
                    return;
                newUser.NickName = nick_message.Sender;
                var ids_list = _list_of_users.Select(x => x.ID);
                while (ids_list.Contains(newUser.ID = rnd.Next(1, 10000))) ;
                SendMessageTo(new("Server", $"Your ID: {newUser.ID}", $"id\x1{newUser.ID}"), newUser);
            }
            newUser.ReaderThread = new(new ParameterizedThreadStart(ReadMessageFrom)) { Name = "ServerAPI_ReadUserThread_" + newUser.ID };

            SendMessageTo(new Message(
                "Server",
                $"Max file size on the server: {MaxFileSize} MiB",
                MessageService.MaxFileSize(MaxFileSize)), newUser);
            lock (_list_of_users_locker)
            {
                SendMessageTo(new("Server", string.Empty, $"new_user\x1{NickName}"), newUser);
                foreach (var i in _list_of_users)
                {
                    SendMessageTo(new("Server", string.Empty, $"new_user\x1{i.FullName}"), newUser);
                }

                _list_of_users.Add(newUser);
                RaisePropertyChanged(nameof(Title));
            }

            SendHistoryToNewUser(newUser);

            SendMessageToAllExcept(new(
                "Server",
                $"New user \"{newUser.FullName}\"",
                $"new_user\x1{newUser.FullName}"), newUser);

            newUser.ReaderThread.Start(newUser);
        }
        private void Listen()
        {
            ServerSocket.Listen(MaxCountOfUsers);
            while (IsWorking)
            {
                User newUser;
                try
                {
                    newUser = new()
                    {
                        Connection = new(ServerSocket.Accept())
                    };
                }
                catch
                {
                    continue;
                }
                if (CurrentCountOfUsers == MaxCountOfUsers)
                {
                    SendMessageTo(new Message("Server", "No places on the server", MessageService.NoPlaces), newUser);
                    newUser.Connection.Disconnect();
                    continue;
                }
                SendMessageTo(new Message("Server", "Successfully connected", "connection_success"), newUser);
                AcceptNewUser(newUser);
            }
        }

        public override void Start()
        {
            if (IsWorking) return;

            ListeningThread = new(new ThreadStart(Listen)) { Name = "ServerAPI_ListenThread" };
            ListeningThread.Start();
            IsWorking = true;
        }
        public override void Shutdown()
        {
            if (!IsWorking) return;

            IsWorking = false;
            SendMessageToAllExcept(new("Server", "Closed", MessageService.Disconnect), null);
            lock (_list_of_users_locker)
            {
                while (CurrentCountOfUsers != 0)
                    DisconnectAndRemoveUser(_list_of_users[0]);
            }
            ServerSocket.Close();
        }

        public ServerAPI(ServerAPIInitArgs new_connection_info) : base()
        {
            _list_of_users = new();
            _list_of_users.CollectionChanged += (s, e) => { RaisePropertyChanged(nameof(ListOfUsers)); };

            localIP = Global.GetLocalIPAddress();
            publicIP = Global.GetExternIPAddress();
            Port = new_connection_info.Port;
            NickName = new_connection_info.NickName;
            MaxFileSize = new_connection_info.MaxFileSize;
            MaxCountOfUsers = new_connection_info.MaxUsersCount;

            ServerSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
        }
    }
}
