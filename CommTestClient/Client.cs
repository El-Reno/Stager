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
            TcpClient client = new TcpClient("127.0.0.1", 8000);
            Stream s = client.GetStream();
            BinaryReader r = new BinaryReader(s);
            BinaryWriter w = new BinaryWriter(s);
            //CommMessage msg = clearChannel.ReceiveMessage();
            CommHeader header = clearChannel.ReceiveHeader(r);

            Console.WriteLine("[*] Command {0}", header.Command); 
            Console.WriteLine("[*] Compression {0}", header.Compression);
            Console.WriteLine("[*] Header Type {0}", header.Type);
            Console.WriteLine("[*] Header Reserved Byte {0}", header.Reserved);
            Console.WriteLine("[*] Header ID {0}", header.Id);
            Console.WriteLine("[*] Data Length {0}", header.DataLength);
            Console.WriteLine("[*] Outputing data");
            int read = 0;
            int chunk = 1024;
            while(read < header.DataLength)
            {
                if(header.DataLength - read < chunk)
                {
                    Console.Out.WriteLine(Encoding.UTF8.GetString(clearChannel.ReceiveBytes(r, header.DataLength - read)));
                    read += header.DataLength - read;
                }
                else
                {
                    Console.Out.WriteLine(Encoding.UTF8.GetString(clearChannel.ReceiveBytes(r, chunk)));
                    read += 1024;
                }
            }
            Console.ReadLine();
        }
    }
}
