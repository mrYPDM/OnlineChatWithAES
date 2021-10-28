using System;
using System.IO;
using System.Security.Cryptography;
using WebAPI;

namespace TcpClientServerChat
{
    static class SecureMessageService
    {
        static public string EncryptToBase64(this Message mesg, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            using ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
            using MemoryStream ms = new();
            using (CryptoStream crypto = new(ms, encryptor, CryptoStreamMode.Write))
            {
                using StreamWriter writer = new(crypto);
                writer.Write(mesg.ToBase64String());
            }
            return Convert.ToBase64String(ms.ToArray());
        }
        static public Message DecryptFromBase64(string encrypted_message, byte[] key, byte[] iv)
        {
            Aes aes = Aes.Create();
            using ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            using MemoryStream ms = new(Convert.FromBase64String(encrypted_message));
            using CryptoStream crypto = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader reader = new(crypto);
            Message mesg = Message.FromBase64String(reader.ReadToEnd());
            return mesg;
        }

        static public string CreateSecureMessage(this Message mesg, byte[] key, byte[] iv)
        {
            string encrypted_message = mesg.EncryptToBase64(key, iv);
            string base64_iv = Convert.ToBase64String(iv);
            using MemoryStream ms = new();
            using (BinaryWriter writer = new(ms))
            {
                writer.Write(encrypted_message);
                writer.Write(base64_iv);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
        static public Message ReadSecureMessage(string secure_message, byte[] key)
        {
            using MemoryStream ms = new(Convert.FromBase64String(secure_message));
            using BinaryReader reader = new(ms);
            string encrypted_message = reader.ReadString();
            byte[] iv = Convert.FromBase64String(reader.ReadString());
            return DecryptFromBase64(encrypted_message, key, iv);
        }
    }
}
