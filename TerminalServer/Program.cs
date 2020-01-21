using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Reno.Comm;

namespace TerminalServer
{
    class Program
    {
        static Dictionary<string, string> arguments;
        static void Main(string[] args)
        {
            Console.Title = "Reno Terminal Server";
            try
            {
                arguments = ParseArgs(args);
            }
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
            TcpListener listener = new TcpListener(IPAddress.Parse(arguments["server"]), Int32.Parse(arguments["port"]));
            listener.Start();
            Console.WriteLine("[*] Starting server");
            TcpClient client = listener.AcceptTcpClient();
            CommChannel channel = new ClearChannel(client, "GZIP");
            TerminalServer server = new TerminalServer(channel);
            server.Start();
        }
        /// <summary>
        /// Prints help menu for the executable
        /// </summary>
        private static void HelpMenu()
        {

        }
        /// <summary>
        /// Parses the arguments that should be supplied to the terminal server.
        /// Throws ArgumentException if the supplied arguments are not correct
        /// </summary>
        /// <param name="args">Arguments supplied to the program</param>
        /// <returns>Dictionary storing the arguments</returns>
        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            Regex allFlags = new Regex("^--server|^--port|^--compression");
            Regex server = new Regex("^--server");
            Regex port = new Regex("^--port");
            Regex compression = new Regex("^--compression");
            int numArgs = args.Length;
            for(int i = 0; i < numArgs; i++)
            {
                // Check if it has a flag we're expecting
                if (!allFlags.IsMatch(args[i]) && i % 2 == 0)   // Make sure even number args are the flags we expect
                    throw new ArgumentException("Using invalid flag: {0}", args[i]);
                else if (server.IsMatch(args[i]) && i < numArgs - 1)
                {
                    IPAddress test; // Test if an IP address is sent
                    if (IPAddress.TryParse(args[i + 1], out test))
                        arguments["server"] = args[i + 1];
                    else
                        throw new ArgumentException("Server provided is not a valid server");
                    i++;
                }
                else if (port.IsMatch(args[i]) && i < numArgs - 1)
                {
                    int test;
                    if (Int32.TryParse(args[i + 1], out test))
                        arguments["port"] = args[i + 1];
                    else
                        throw new ArgumentException("Port provided is not an integer");
                    i++;
                }
                else if (compression.IsMatch(args[i]) && i < numArgs - 1)
                {
                    Regex comp = new Regex("GZIP");
                    if (comp.IsMatch(args[i+1]))
                        arguments["compression"] = args[i + 1];
                    else
                        throw new ArgumentException("Unsupported compression type");
                    i++;
                }
            }
            return arguments;
        }
    }
}
