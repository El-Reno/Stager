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
                ClearChannel channel = new ClearChannel(client);
                // Need command, compression, length of data, and data
                int command = CommChannel.LS | CommChannel.DEFLATE;
                string dir = DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\GitHub\AdventOfCode", "TEXT");
                Console.WriteLine(dir.Length);
                //char[] data = "Some random text".ToCharArray();
                char[] data = dir.ToCharArray();
                int data_len = data.Length;
                CommandHeader h = new CommandHeader(command, data_len);
                Console.WriteLine("Header command {0}", h.Command);
                Console.WriteLine("Header compression {0}", h.Compression);
                Console.WriteLine("Header data len {0}", h.DataLength);
                channel.SendHeader(h);
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
