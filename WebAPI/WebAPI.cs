using System;
using System.Collections.ObjectModel;
using System.IO;
using Prism.Mvvm;

namespace WebAPI
{
    abstract public class WebAPI : BindableBase
    {
        public virtual string Title
        {
            get
            {
                if (IsWorking)
                    return "Chat - Online";
                return "Chat - Offline";
            }
        }

        protected bool _is_working = false;
        public bool IsWorking
        {
            get => _is_working;
            protected set
            {
                _is_working = value;
                RaisePropertyChanged(nameof(IsWorking));
                RaisePropertyChanged(nameof(Title));
            }
        }

        protected string _nick;
        public virtual string NickName
        {
            get => _nick;
            set => _nick = value;
        }

        protected int _max_file_size;
        public virtual int MaxFileSize
        {
            get => _max_file_size;
            set => _max_file_size = value;
        }

        public virtual ReadOnlyCollection<string> ListOfUsers { get; protected set; }

        protected ObservableCollection<Message> _list_of_messages;
        public ReadOnlyObservableCollection<Message> ListOfMessages { get; protected set; }

        protected readonly object _list_of_messages_locker = 0;
        protected void AddNewMessage(Message message)
        {
            lock (_list_of_messages_locker)
            {
                _list_of_messages.Add(message);
            }
            RaisePropertyChanged(nameof(ListOfMessages));
        }

        public abstract void SendMessage(Message message);
        public virtual void SendText(string text) => SendMessage(new(NickName, text, string.Empty));

        protected void CheckFile(string filename, int max_size)
        {
            FileInfo info = new(filename);
            if (!info.Exists)
                throw new FileNotFoundException($"{info.Name} does not exists");
            if (info.Length / 1024 / 1024 > max_size)
                throw new Exception($"{info.Name} is larger than {max_size} MiB");
        }
        public void SendFile(string filepath)
        {
            try
            {
                CheckFile(filepath, MaxFileSize);
            }
            catch (Exception e)
            {
                AddNewMessage(new Message("Server", e.Message, "erorr"));
                return;
            }
            Message mesg = new(NickName, filepath);
            SendMessage(mesg);
            mesg.OtherData = filepath;
            AddNewMessage(mesg);
            GC.Collect();
        }
        protected void ReadFile(Message message)
        {
            var file_bytes = Convert.FromBase64String(message.OtherData);
            if (!Directory.Exists(GlobalSettings.CacheFolder))
                Directory.CreateDirectory(GlobalSettings.CacheFolder);
            message.OtherData = $"{GlobalSettings.CacheFolder}{message.Text}";
            File.WriteAllBytes(message.OtherData, file_bytes);
            GC.Collect();
        }

        public abstract void Start();
        public abstract void Shutdown();

        public WebAPI()
        {
            Directory.CreateDirectory(GlobalSettings.CacheFolder);
            _list_of_messages = new();
            ListOfMessages = new(_list_of_messages);
        }
    }
}
