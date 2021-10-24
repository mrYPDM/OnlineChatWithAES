using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;

using TcpClientServerChat;
using WebAPI;

namespace TiMP_Project_OnlineChat
{
    class MainWindowViewModel : BindableBase
    {
        private SecureWebAPI model = null;
        private NewConnectionWindowViewModel server_connection = null;
        private NewConnectionWindowViewModel client_connection = null;
        public string Title => model?.Title ?? "Chat - Offline";
        public bool IsWorking => model != null && model.IsWorking;
        public ReadOnlyCollection<string> ListOfUsers => model?.ListOfUsers;
        public string TextMessage { get; set; }
        public ReadOnlyObservableCollection<Message> ListOfMessages => model?.ListOfMessages;

        public DelegateCommand OpenStartServerWindow { get; }
        public DelegateCommand OpenConnectToServerWindow { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand OpenSettingsWindowCommand { get; }

        public DelegateCommand SendMessageCommand { get; }
        public DelegateCommand SendFileCommand { get; }


        public DelegateCommand<Message> AddNewMessageUI { get; set; }
        public DelegateCommand ClearMessagesUI { get; set; }

        private bool SureToDisconnect()
        {
            return
                MessageBox.Show(
                           "Есть существующее подключение.\nВы уверены, что хотите отключиться?",
                           "Внимание!",
                           MessageBoxButton.YesNo,
                           MessageBoxImage.Warning)
                == MessageBoxResult.Yes;
        }

        public MainWindowViewModel()
        {
            OpenStartServerWindow = new(() =>
            {
                if (IsWorking)
                    if (!SureToDisconnect()) return;

                server_connection ??= new(false);
                NewConnectionWindow newConnectionWindow = new(server_connection);

                if (newConnectionWindow.ShowDialog() == false) return;

                model?.Shutdown();
                ClearMessagesUI?.Execute();

                model = new ServerAPI(new()
                {
                    Port = server_connection.Port,
                    NickName = server_connection.NickName,
                    MaxFileSize = server_connection.MaxFileSize,
                    MaxUsersCount = server_connection.MaxUsersCount
                });

                model.PropertyChanged += Model_PropertyChanged;
                model.Start();
            });

            OpenConnectToServerWindow = new(() =>
            {
                if (IsWorking)
                    if (!SureToDisconnect()) return;

                client_connection ??= new(true);
                NewConnectionWindow newConnectionWindow = new(client_connection);

                if (newConnectionWindow.ShowDialog() == false) return;

                model?.Shutdown();
                ClearMessagesUI?.Execute();
                model = new ClientAPI(new()
                { 
                    IP = client_connection.IP,
                    Port = client_connection.Port,
                    NickName = client_connection.NickName
                });

                model.PropertyChanged += Model_PropertyChanged;
                model.Start();
            });

            DisconnectCommand = new(() =>
            {
                model?.Shutdown();

                if (server_connection != null)
                    server_connection.IsStopped = true;
                if (client_connection != null)
                    client_connection.IsStopped = true;
            });

            OpenSettingsWindowCommand = new(() => 
            {
                if (model is ServerAPI serverModel)
                {
                    NewConnectionWindow window = new(server_connection);
                    if (window.ShowDialog() != true) return;

                    serverModel.MaxFileSize = server_connection.MaxFileSize;
                    serverModel.NickName = server_connection.NickName;
                    if ((server_connection.MaxUsersCount == 0 ? int.MaxValue : server_connection.MaxUsersCount) < serverModel.CurrentCountOfUsers)
                    {
                        if (MessageBox.Show(
                            "Количество пользователей на сервере больше, чем новое значение.\nПродолжить?",
                            "Внимание!",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            serverModel.MaxCountOfUsers = server_connection.MaxUsersCount;
                    }
                    else serverModel.MaxCountOfUsers = server_connection.MaxUsersCount;
                }
                else if (model is ClientAPI)
                {
                    NewConnectionWindow window = new(client_connection);
                    if (window.ShowDialog() != true) return;

                    model.NickName = client_connection.NickName;
                }
            });

            SendMessageCommand = new(() =>
            {
                if (model != null && !string.IsNullOrWhiteSpace(TextMessage))
                {
                    model.SendText(TextMessage);
                    TextMessage = string.Empty;
                    RaisePropertyChanged(nameof(TextMessage));
                }
            });

            SendFileCommand = new(() =>
            {
                if (model == null) return;

                Microsoft.Win32.OpenFileDialog ofd = new()
                {
                    Multiselect = true,
                    Title = "Choose file(s)",
                    CheckFileExists = true,
                };
                if (ofd.ShowDialog() != true) return;

                new Thread(() =>
                {
                    foreach (var file in ofd.FileNames)
                    {
                        model.SendFile(file);
                    }
                }).Start();
            });
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ListOfMessages))
            {
                if (ListOfMessages?.Count == 0)
                    ClearMessagesUI?.Execute();
                else if (ListOfMessages?.Last().Text != string.Empty)
                    AddNewMessageUI?.Execute(ListOfMessages.Last());
            }
            else if (e.PropertyName == nameof(IsWorking))
            {
                if (server_connection != null)
                    server_connection.IsStopped = !IsWorking;
                if (client_connection != null)
                    client_connection.IsStopped = !IsWorking;
            }
            RaisePropertyChanged(e.PropertyName);
        }
    }
}
