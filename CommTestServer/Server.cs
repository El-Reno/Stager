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
                /*Stream s = client.GetStream();
                BinaryReader r = new BinaryReader(s);
                BinaryWriter w = new BinaryWriter(s);*/
                Console.WriteLine("[*] Accepted client: " + client.Client.RemoteEndPoint.ToString());
                Console.WriteLine("[*] Creating test command");
                ClearChannel channel = new ClearChannel(client, "GZIP");
                Random rand = new Random((int)DateTimeOffset.Now.ToUnixTimeMilliseconds());
                // Need command, compression, length of data, and data
                byte command = CommChannel.LS;
                int id = rand.Next();
                byte compression = CommChannel.DEFLATE;
                byte hType = CommChannel.COMMAND;
                byte reserved = 0b00001111;
                string dir = DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\GitHub\AdventOfCode", "TEXT");
                Console.WriteLine(dir.Length);
                //char[] data = "Some random text".ToCharArray();
                char[] data = dir.ToCharArray();
                byte[] compressed = channel.Compress(Encoding.UTF8.GetBytes(data));
                int data_len = compressed.Length;
                CommHeader h = new CommHeader(command, compression, hType, reserved, id, data_len);
                Console.WriteLine("Header command {0}", h.Command);
                Console.WriteLine("Header compression {0}", h.Compression);
                Console.WriteLine("Header Type {0}", h.Type);
                Console.WriteLine("Header Reserved byte {0}", h.Reserved);
                Console.WriteLine("Header ID {0}", h.Id);
                Console.WriteLine("Header data len {0}", h.DataLength);

                channel.SendHeader(h);
                //channel.SendBytes(Encoding.UTF8.GetBytes(data));
                channel.SendBytes(compressed);
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
