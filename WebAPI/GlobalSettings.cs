using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace WebAPI
{
    public static class GlobalSettings
    {
        static public string CacheFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OnlineChat/";
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
            try
            {
                return new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            } catch
            {
                return "null";
            }
        }
    }
}
