using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerClass server = new ServerClass("127.0.0.1", 11000);
            server.Listen(10);
        }
    }
}
