using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace WebAPI
{
    public static class Global
    {
        static public string CacheFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/TIMP_ONLINECHAT/";

        /// <summary>
        /// Проверяет возможность файла для отправки
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="max_size"></param>
        /// <exception cref="FileNotFoundException">Если filename не существует</exception>
        /// <exception cref="Exception">Если размер файла больше max_size</exception>
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                    .Where(x =>
                        x.ToString().Contains("192.168") &&
                        x.AddressFamily == AddressFamily.InterNetwork)
                    .FirstOrDefault()?.ToString();
        }
        public static string GetExternIPAddress()
        {
            return "null";
            //return new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
        }
    }
}
