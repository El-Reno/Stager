﻿using System;
using System.Net;
using System.Net.Sockets;
using Reno.Comm;

namespace TerminalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
            listener.Start();
            Console.WriteLine("[*] Starting server");
            TcpClient client = listener.AcceptTcpClient();
            CommChannel channel = new ClearChannel(client, "NONE");
            TerminalServer server = new TerminalServer(channel, CommChannel.GZIP);
            server.Start();
        }
    }
}
