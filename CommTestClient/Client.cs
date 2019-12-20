using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Reno.Comm;

namespace CommTestClient
{
    class Client
    {
        static void Main(string[] args)
        {
            /*TcpClient client = new TcpClient("127.0.0.1", 8000);
            int bytes = -1;
            do
            {
                Console.WriteLine("Byte received: " + client.GetStream().ReadByte());
            } while (bytes != 0);
            */
            ClearChannel clearChannel = new ClearChannel("127.0.0.1", 8000, "NONE");
            byte[] bytes = clearChannel.ReceiveMessage();
            int command = 0;
            int length = 0;
            string data = "";
            // Parse the message
            using (var memStream = new MemoryStream(bytes))
            {
                using(var binStream = new BinaryReader(memStream))
                {
                    command = IPAddress.NetworkToHostOrder(binStream.ReadInt32());
                    length = IPAddress.NetworkToHostOrder(binStream.ReadInt32());
                    data = Encoding.UTF8.GetString(binStream.ReadBytes(length));
                }
            }
            Console.WriteLine("[*] Command {0}", command & 0x000F);
            Console.WriteLine("[*] Compression {0}", command & 0x00F0);
            Console.WriteLine("[*] Data Length {0}", length);
            Console.WriteLine("[*] Data {0}", data);
            Console.ReadLine();
        }
    }
}
