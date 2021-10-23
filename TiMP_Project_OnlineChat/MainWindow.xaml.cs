using System.Windows;
using System.Windows.Controls;

namespace TiMP_Project_OnlineChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (MessagesScroll.VerticalOffset == MessagesScroll.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                MessagesScroll.ScrollToVerticalOffset(MessagesScroll.ExtentHeight);
            }
        }
    }
}
