using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WebAPI;

namespace OnlineChat
{
    public partial class MessageUI : UserControl
    {
        public MessageUI(Message mesg)
        {
            InitializeComponent();
            Message = mesg;

            CreatorBlock.Text = Message.Sender;
            if (Message.Service == MessageService.File)
            {
                Hyperlink link = new()
                {
                    NavigateUri = new Uri(Message.OtherData)
                };
                link.RequestNavigate += (s, e) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Uri.UnescapeDataString((s as Hyperlink).NavigateUri.AbsolutePath),
                            UseShellExecute = true
                        });
                    } catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                link.Inlines.Add(Message.Text);
                DataBlock.Inlines.Add(link);
            }
            else
            {
                DataBlock.Text = Message.Text;
            }
        }

        public Message Message { get; }

        private void CopyTextButon_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText($"{CreatorBlock.Text}: {DataBlock.Text}");
        }
    }
}
