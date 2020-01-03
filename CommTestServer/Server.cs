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
using Reno.Stages;
using System.Reflection;

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
                string dir = DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\GitHub\AdventOfCode", "TEXT");
                Console.WriteLine(dir.Length);
                //char[] data = "Some random text".ToCharArray();
                char[] data = dir.ToCharArray();
                int data_len = data.Length;
                int data_header_len = sizeof(int) + sizeof(int);
                byte[] data_header = new byte[data_header_len];
                byte[] data_buffer = new byte[data_len];
                using (var memStream = new MemoryStream())
                {
                    using (var binStream = new BinaryWriter(memStream))
                    {
                        binStream.Write(IPAddress.HostToNetworkOrder(command));
                        binStream.Write(IPAddress.HostToNetworkOrder(data_len));
                    }
                    data_buffer = memStream.ToArray();
                }
                client.GetStream().Write(data_buffer, 0, data_header_len);
                //client.GetStream().Write(Encoding.UTF8.GetBytes(data), 0, data_len);
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
