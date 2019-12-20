using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Reno.Comm;

namespace CommTestServer
{
    class Server
    {
        static void Main(string[] args)
        {
            try
            {
                TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
                Console.WriteLine("[*] Starting server");
                server.Start();
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("[*] Accepted client: " + client.Client.RemoteEndPoint.ToString());
                Console.WriteLine("[*] Creating test command");
                // Need command, compression, length of data, and data
                int command = CommChannel.LS | CommChannel.DEFLATE;
                char[] data = "Some random text".ToCharArray();
                int data_len = data.Length;
                Console.WriteLine("String length: {0}", data_len);
                int total_len = sizeof(int) + sizeof(int) + data_len;
                byte[] data_buffer = new byte[total_len];
                using (var memStream = new MemoryStream())
                {
                    using (var binStream = new BinaryWriter(memStream))
                    {
                        binStream.Write(IPAddress.HostToNetworkOrder(command));
                        binStream.Write(IPAddress.HostToNetworkOrder(data_len));
                        binStream.Write(data);
                    }
                    data_buffer = memStream.ToArray();
                }
                client.GetStream().Write(data_buffer, 0, total_len);
                client.Close();
                server.Stop();
            }
            catch (ArgumentOutOfRangeException e) { }
            Console.ReadLine();
        }

        static void WriteNetworkOrder(BinaryWriter s, int value)
        {
            s.Write(IPAddress.HostToNetworkOrder(value));
        }
    }
}
