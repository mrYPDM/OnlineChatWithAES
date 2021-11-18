using System;
using System.IO;

namespace WebAPI
{
    public class Message
    {
        public string Sender { get; init; } = string.Empty;
        public string Service { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public string OtherData { get; set; } = string.Empty;
        private Message() { }

        public Message(string sender, string text, string cmd)
        {
            Service = cmd;
            Sender = sender;
            Text = text;
        }
        public Message(string sender, string filename)
        {
            Service = MessageService.File;
            Sender = sender;
            Text = Path.GetFileName(filename);
            OtherData = Convert.ToBase64String(File.ReadAllBytes(filename));
        }

        public static Message FromBase64String(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            try
            {
                using MemoryStream ms = new(Convert.FromBase64String(str));
                using BinaryReader reader = new(ms);
                Message mesg = new()
                {
                    Sender = reader.ReadString(),
                    Service = reader.ReadString(),
                    Text = reader.ReadString(),
                    OtherData = reader.ReadString(),
                };
                return mesg;
            }
            catch
            {
                return null;
            }
        }
        public string ToBase64String()
        {
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);
            writer.Write(Sender);
            writer.Write(Service);
            writer.Write(Text);
            writer.Write(OtherData);
            writer.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }
    }
}
