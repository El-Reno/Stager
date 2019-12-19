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
            for(int i = 0; i < bytes.Length; i++)
            {
                Console.WriteLine("Byte received: " + bytes[i]);
            }
            Console.ReadLine();
        }
    }
}
