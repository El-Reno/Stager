using System;
using System.Collections.Generic;
using System.Text;
using Reno.Comm;

namespace TerminalServer
{
    class TerminalServer
    {
        CommChannel channel;
        byte compression;
        string prompt;
        public TerminalServer(CommChannel channel, byte compression)
        {
            this.channel = channel;
            this.compression = compression;
            prompt = Environment.MachineName + ">";
        }

        public void Start()
        {
            Random r = new Random();
            bool run = true;
            while (run)
            {
                Console.Write(prompt);
                string command = Console.ReadLine();
                if(command.Equals('\t'))
                {
                    Console.WriteLine("TAB");
                }
                string[] commandString = command.Split(" ");
                switch (commandString[0])
                {
                    case "EXIT":
                        run = false;
                        CommHeader a = CreateHeader(CommChannel.EXIT, compression, CommChannel.COMMAND, r.Next(), 0);
                        channel.SendHeader(a);
                        break;
                    case "exit":
                        run = false;
                        CommHeader b = CreateHeader(CommChannel.EXIT, compression, CommChannel.COMMAND, r.Next(), 0);
                        channel.SendHeader(b);
                        break;
                    case "LS":
                        ListDirectory(commandString, r);
                        break;
                    case "ls":
                        ListDirectory(commandString, r);
                        break;
                    case "PWD":
                        PresentWorkingDirectory(r);
                        break;
                    case "pwd":
                        PresentWorkingDirectory(r);
                        break;
                    case "CD":
                        ChangeDirectory(commandString, r);
                        break;
                    case "cd":
                        ChangeDirectory(commandString, r);
                        break;
                    default:
                        break;
                }
                if (run)
                {
                    CommHeader responseHeader = channel.ReceiveHeader();
                    //byte[] response = channel.ReceiveBytes(responseHeader.DataLength);
                    string sResponse = "";
                    if (responseHeader.Compression == CommChannel.GZIP)
                    {
                        byte[] response = channel.Decompress(channel.ReceiveBytes(responseHeader.DataLength));
                        sResponse = Encoding.UTF8.GetString(response);
                    }
                    else
                    {
                        byte[] response = channel.ReceiveBytes(responseHeader.DataLength);
                        sResponse = Encoding.UTF8.GetString(response);
                    }

                    Console.WriteLine(sResponse);
                }
            }
        }
        private void ListDirectory(string[] commandString, Random r)
        {
            // Check for a directory argument
            if (commandString.Length > 1)
            {
                string dir = commandString[1];
                int len = dir.Length;
                CommHeader c = CreateHeader(CommChannel.LS, compression, CommChannel.COMMAND, r.Next(), len);
                channel.SendHeader(c);
                channel.SendBytes(Encoding.UTF8.GetBytes(dir));
            }
            else
            {
                CommHeader c = CreateHeader(CommChannel.LS, compression, CommChannel.COMMAND, r.Next(), 0);
                channel.SendHeader(c);
            }
        }
        private void ChangeDirectory(string[] commandString, Random r)
        {
            // Check for a directory argument
            if (commandString.Length > 1)
            {
                string dir = commandString[1];
                int len = dir.Length;
                CommHeader c = CreateHeader(CommChannel.CD, compression, CommChannel.COMMAND, r.Next(), len);
                channel.SendHeader(c);
                channel.SendBytes(Encoding.UTF8.GetBytes(dir));
            }
            else
            {
                CommHeader c = CreateHeader(CommChannel.CD, compression, CommChannel.COMMAND, r.Next(), 0);
                channel.SendHeader(c);
            }
        }
        private void PresentWorkingDirectory(Random r)
        {
            CommHeader lowPWDHeader = CreateHeader(CommChannel.PWD, compression, CommChannel.COMMAND, r.Next(), 0);
            channel.SendHeader(lowPWDHeader);
        }
        private string[] ParseCommand(string command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new CommHeader for transmission
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="compression">Compression method that is used</param>
        /// <param name="type">Type of message: Command or Response</param>
        /// <param name="id">Message Id</param>
        /// <param name="data_length">Length of data</param>
        /// <returns></returns>
        private CommHeader CreateHeader(byte command, byte compression, byte type, int id, int data_length)
        {
            return new CommHeader(command, compression, type, 0, id, data_length);
        }
    }
}
