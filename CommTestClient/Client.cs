using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Reno.Comm;
using Reno.Stages;

namespace CommTestClient
{
    class Client
    {
        static void Main(string[] args)
        {
            ClearChannel clearChannel = new ClearChannel("127.0.0.1", 8000, "NONE");
            //CommMessage msg = clearChannel.ReceiveMessage();
            CommandHeader header = clearChannel.ReceiveHeader();

            Console.WriteLine("[*] Command {0}", header.Command);
            Console.WriteLine("[*] Compression {0}", header.Compression);
            Console.WriteLine("[*] Data Length {0}", header.DataLength);
            Console.ReadLine();
        }
    }
}
