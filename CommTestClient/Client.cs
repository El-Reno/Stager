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
            int command = 0;
            int length = 0;
            string data = "";
            // Parse the message
            using (var memStream = new MemoryStream(header.GetBytes))
            {
                using(var binStream = new BinaryReader(memStream))
                {
                    command = IPAddress.NetworkToHostOrder(binStream.ReadInt32());
                    length = IPAddress.NetworkToHostOrder(binStream.ReadInt32());
                }
            }
            Console.WriteLine("[*] Command {0}", command & 0x000F);
            Console.WriteLine("[*] Compression {0}", command & 0x00F0);
            Console.WriteLine("[*] Data Length {0}", length);
            /*using (var memStream = new MemoryStream(msg.Message))
            {
                using (var binStream = new BinaryReader(memStream))
                {
                    data = Encoding.UTF8.GetString(binStream.ReadBytes(length));
                }
            }
            Console.WriteLine("[*] Data {0}", data);*/
            Console.ReadLine();
        }
    }
}
