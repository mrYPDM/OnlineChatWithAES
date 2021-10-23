using WebAPI;

namespace TiMP_Project_OnlineChat
{
    public class NewConnectionWindowViewModel
    {
        private readonly bool is_user;
        public bool IsUserConnection => is_user;
        public bool IsServerConnection => !is_user;

        public string Title
        {
            get
            {
                if (IsStopped)
                {
                    return is_user ? "Connect to server" : "Create server";
                }
                return is_user ? "Connection settings" : "Server settings";
            }
        }

        public bool IsIPBoxEnable => IsUserConnection && IsStopped;

        public bool IsStopped { get; set; } = true;

        public int Port { get; set; }
        public string IP { get; set; }
        public string NickName { get; set; }
        public int MaxUsersCount { get; set; }
        public int MaxFileSize { get; set; }

        public NewConnectionWindowViewModel(bool is_user)
        {
            this.is_user = is_user;
            if (!is_user)
            {
                IP = Global.GetLocalIPAddress() + ", " + Global.GetExternIPAddress();
                Port = 7777;
                NickName = "admin";
                MaxUsersCount = 50;
                MaxFileSize = 20;
            }
        }
    }
}
