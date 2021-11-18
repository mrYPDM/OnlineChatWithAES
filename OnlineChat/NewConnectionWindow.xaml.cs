using System.Windows;

namespace OnlineChat
{
    public partial class NewConnectionWindow : Window
    {
        public NewConnectionWindow(NewConnectionWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if ((DataContext as NewConnectionWindowViewModel).IsDataValid())
            {
                DialogResult = true;
                Close();
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
