using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CommTestServer
{
    class Server
    {
        static void Main(string[] args)
        {
            try
            {
                TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
                Console.WriteLine("Starting server");
                server.Start();
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Accepted client: " + client.Client.RemoteEndPoint.ToString());
                byte[] data = new byte[16];
                for (int i = 0; i < data.Length; i++)
                    data[i] = (byte)i;
                client.GetStream().Write(data, 0, data.Length);
                client.Close();
                server.Stop();
            }
            catch (ArgumentOutOfRangeException e) { }
            Console.ReadLine();
        }
    }
}
