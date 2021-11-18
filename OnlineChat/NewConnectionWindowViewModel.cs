using System.Windows;
using WebAPI;

namespace OnlineChat
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

        public bool IsDataValid()
        {
            if (string.IsNullOrWhiteSpace(IP) && is_user)
            {
                MessageBox.Show("Введите IP адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (Port < 0 || Port > 65535)
            {
                MessageBox.Show("Порт должен быть в диапазоне [0; 65535]", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NickName))
            {
                MessageBox.Show("Введите NickName", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!is_user)
            {
                if (MaxUsersCount < 0)
                {
                    MessageBox.Show("Количество пользователей должно быть не меньше 0 (0 - неограниченно)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (MaxFileSize < 0)
                {
                    MessageBox.Show("Размер файла должен быть не меньше 0 (0 - неограничено)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        public NewConnectionWindowViewModel(bool is_user)
        {
            this.is_user = is_user;
            if (!is_user)
            {
                IP = GlobalSettings.GetLocalIPAddress() + ", " + GlobalSettings.GetExternIPAddress();
                Port = 7777;
                NickName = "admin";
                MaxUsersCount = 50;
                MaxFileSize = 20;
            }
        }
    }
}
