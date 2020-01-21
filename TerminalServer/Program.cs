﻿using System;
using System.Net;
using System.Net.Sockets;
using Reno.Comm;
using Reno.Utilities;

namespace TerminalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Reno Terminal Server";
            TcpListener listener = new TcpListener(IPAddress.Parse("192.168.1.186"), 8888);
            listener.Start();
            Console.WriteLine("[*] Starting server");
            TcpClient client = listener.AcceptTcpClient();
            CommChannel channel = new ClearChannel(client, "GZIP");
            TerminalServer server = new TerminalServer(channel, CommChannel.GZIP);
            server.Start();
        }
    }
}
