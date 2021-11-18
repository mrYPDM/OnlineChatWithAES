using System.Windows;
using System.Windows.Controls;

namespace OnlineChat
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            VM = DataContext as MainWindowViewModel;
            VM.AddNewMessageUI = new(mesg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageUI mesg_ui = new(mesg);
                    MessagesStack.Children.Add(mesg_ui);
                });
            });

            VM.ClearMessagesUI = new(() => MessagesStack.Children.Clear());
        }

        readonly MainWindowViewModel VM;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VM.DisconnectCommand.Execute();
        }


        private bool AutoScroll = true;
        private void MessagesScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {    
                if (MessagesScroll.VerticalOffset == MessagesScroll.ScrollableHeight)
                    AutoScroll = true;
                else
                    AutoScroll = false;
            }
            if (AutoScroll && e.ExtentHeightChange != 0)
                MessagesScroll.ScrollToVerticalOffset(MessagesScroll.ExtentHeight);
        }
    }
}
