using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI
{
    public interface IConnection
    {
        bool Connected { get; }

        void SendMessage(string message);
        string ReadMessage();

        void Connect();
        void Disconnect();
    }
}
