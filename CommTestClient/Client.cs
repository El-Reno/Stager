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
using System.IO.Compression;

namespace CommTestClient
{
    class Client
    {
        static void Main(string[] args)
        {
            /*ClearChannel clearChannel = new ClearChannel("127.0.0.1", 8000, "GZIP");
            //TcpClient client = new TcpClient("127.0.0.1", 8000);
            //ClearChannel clearChannel = new ClearChannel(client);
            CommHeader header = clearChannel.ReceiveHeader();

            Console.WriteLine("[*] Command {0}", header.Command); 
            Console.WriteLine("[*] Compression {0}", header.Compression);
            Console.WriteLine("[*] Header Type {0}", header.Type);
            Console.WriteLine("[*] Header Reserved Byte {0}", header.Reserved);
            Console.WriteLine("[*] Header ID {0}", header.Id);
            Console.WriteLine("[*] Data Length {0}", header.DataLength);
            Console.WriteLine("[*] Outputing data");
            int read = 0;
            int chunk = 1024;
            byte[] message = new byte[header.DataLength];
            using (var m = new MemoryStream(message))
            {
                using (var b = new BinaryWriter(m))
                {
                    while (read < header.DataLength)
                    {
                        if (header.DataLength - read < chunk)
                        {
                            //Console.Out.WriteLine(Encoding.UTF8.GetString(clearChannel.ReceiveBytes(header.DataLength - read)));
                            b.Write(clearChannel.ReceiveBytes(header.DataLength - read));
                            read += header.DataLength - read;
                        }
                        else
                        {
                            //Console.Out.WriteLine(Encoding.UTF8.GetString(clearChannel.ReceiveBytes(chunk)));
                            b.Write(clearChannel.ReceiveBytes(chunk));
                            read += 1024;
                        }
                    }
                }
            }
            Console.WriteLine("Decompressing");
            byte[] decompressed = clearChannel.Decompress(message);
            Console.WriteLine(Encoding.UTF8.GetString(decompressed));
            Console.ReadLine();
            */
            ClearChannel clearChannel = new ClearChannel("192.168.1.186", 8888, "NONE");
            Terminal terminal = new Terminal(clearChannel);
            terminal.Execute();
            
        }
    }
}
