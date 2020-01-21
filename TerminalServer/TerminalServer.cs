﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Reno.Comm;
using Reno.Utilities;

namespace TerminalServer
{
    /// <summary>
    /// The TerminalServer allows an operator to issue commands to a machine running the Terminal.
    /// The TerminalServer provides an extremely basic command line for issueing commands.
    /// </summary>
    class TerminalServer
    {
        CommChannel channel;
        byte compression;
        bool isCompressed;
        string prompt, localPWD;
        /// <summary>
        /// Creates a new TerminalServer for the terminal program.
        /// The operator interacts with the server and the server interacts with the client machine
        /// </summary>
        /// <param name="channel">Comm channel for client/server communication</param>
        /// <param name="compression">Compression method - GZIP, DEFLATE, or NONE</param>
        public TerminalServer(CommChannel channel, byte compression)
        {
            this.channel = channel;
            this.compression = compression;
            if(compression == 0)
            {
                isCompressed = false;
            }
            else
            {
                isCompressed = true;
            }
            prompt = Environment.MachineName + ">";
            localPWD = Directory.GetCurrentDirectory();
        }
        /// <summary>
        /// Starts the terminal server loop. This method reads a command from the command line and then parses the result.
        /// The resulting command and arguments are then translated into commands to be sent over the wire.
        /// Finally, it waits for a response and outputs the results
        /// 
        /// TODO:
        /// Properly parse commands with a method other than string.split
        /// Track sessions - long term
        /// </summary>
        public void Start()
        {
            Random r = new Random();
            bool run = true;
            bool expectReturn = false;
            string[] commandString;
            while (run)
            {
                Console.Write(prompt);
                string command = Console.ReadLine();
                if (command.Equals(""))
                    commandString = new string[] { "" };
                else
                    commandString = Utility.ParseCommand(command);
                switch (commandString[0])
                {
                    case "EXIT":
                        run = false;
                        CommHeader a = CreateHeader(CommChannel.EXIT, compression, CommChannel.COMMAND, r.Next(), 0);
                        channel.SendHeader(a);
                        channel.Close();
                        break;
                    case "exit":
                        run = false;
                        CommHeader b = CreateHeader(CommChannel.EXIT, compression, CommChannel.COMMAND, r.Next(), 0);
                        channel.SendHeader(b);
                        channel.Close();
                        break;
                    case "LS":
                        ListDirectory(commandString, r);
                        expectReturn = true;
                        break;
                    case "ls":
                        ListDirectory(commandString, r);
                        expectReturn = true;
                        break;
                    case "PWD":
                        PresentWorkingDirectory(r);
                        expectReturn = true;
                        break;
                    case "pwd":
                        PresentWorkingDirectory(r);
                        expectReturn = true;
                        break;
                    case "CD":
                        ChangeDirectory(commandString, r);
                        expectReturn = true;
                        break;
                    case "cd":
                        ChangeDirectory(commandString, r);
                        expectReturn = true;
                        break;
                    case "DELETE":
                        DeleteFilesysObject(commandString, r);
                        expectReturn = true;
                        break;
                    case "delete":
                        DeleteFilesysObject(commandString, r);
                        expectReturn = true;
                        break;
                    case "PS":
                        ProcessList(r);
                        expectReturn = true;
                        break;
                    case "ps":
                        ProcessList(r);
                        expectReturn = true;
                        break;
                    case "NETSTAT":
                        Netstat(r);
                        expectReturn = true;
                        break;
                    case "netstat":
                        Netstat(r);
                        expectReturn = true;
                        break;
                    case "UPLOAD":

                        expectReturn = true;
                        break;
                    case "upload":

                        expectReturn = true;
                        break;
                    case "DOWNLOAD":
                        DownloadFile(commandString[1], r);
                        expectReturn = false;   // Helper function handles the return data
                        break;
                    case "download":
                        DownloadFile(commandString[1], r);
                        expectReturn = false; // Helper function handles the return data
                        break;
                    case "LPWD":
                        Console.WriteLine(localPWD);
                        break;
                    case "lpwd":
                        Console.WriteLine(localPWD);
                        break;
                    case "LCD":
                        if (Directory.Exists(commandString[1]))
                        {
                            localPWD = commandString[1];
                        }
                        Console.WriteLine(localPWD);
                        break;
                    case "lcd":
                        if (Directory.Exists(commandString[1]))
                        {
                            localPWD = commandString[1];
                        }
                        Console.WriteLine(localPWD);
                        break;
                    default:
                        continue;
                }
                if (run && expectReturn)
                {
                    CommHeader responseHeader = channel.ReceiveHeader();
                    string sResponse = "";
                    byte[] response = channel.Decompress(channel.ReceiveBytes(responseHeader.DataLength));
                    sResponse = Encoding.UTF8.GetString(response);
                    Console.WriteLine(sResponse);
                }
                expectReturn = false;
            }
        }
        
        private void UploadFile(FileInfo file, Random r)
        {

        }

        private void DownloadFile(string file, Random r)
        {
            // Transmit the command
            byte[] bytes = channel.Compress(Encoding.UTF8.GetBytes(file));
            CommHeader downloadHeader = CreateHeader(CommChannel.DOWNLOAD, compression, CommChannel.COMMAND, r.Next(), bytes.Length);
            channel.SendHeader(downloadHeader);
            channel.SendBytes(bytes);

            // Receive the file
            CommHeader response = channel.ReceiveHeader();
            long size = response.DataLength; // This field is an int, eventually will be a long
            long read = 0;
            // Read the file and save it, then decompress as needed
            FileInfo tmp = new FileInfo(localPWD + "\\tmp");
            if (response.Type == CommChannel.RESPONSE) {
                using (FileStream fileStream = new FileStream(tmp.FullName, FileMode.Create))
                {
                    while (read < size)
                    {
                        try
                        {
                            if (size - read < CommChannel.CHUNK_SIZE)
                            {
                                byte[] b = channel.ReceiveBytes((int)(size - read));
                                fileStream.Seek(read, SeekOrigin.Begin);
                                fileStream.Write(b, 0, b.Length);
                                read += b.Length;
                                DownloadStatus(read, size, file);
                                Console.Write("\n");
                            }
                            else
                            {
                                byte[] b = channel.ReceiveBytes(CommChannel.CHUNK_SIZE);
                                fileStream.Seek(read, SeekOrigin.Begin);
                                fileStream.Write(b, 0, b.Length);
                                read += b.Length;
                                DownloadStatus(read, size, file);
                            }
                        }
                        catch (IOException ioEx)
                        {
                            Console.WriteLine("Error with filestream: {0}", ioEx.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error during download: {0}", e.Message);
                        }
                    }
                }
                // Now decompress the file if required
                string fullFilePath = GetFullPath(file);
                // Rename the file if no compression, otherwise, decompress
                if (response.Compression == CommChannel.NONE)
                {
                    if (File.Exists(tmp.FullName))
                        File.Move(tmp.FullName, fullFilePath);
                }
                else
                {
                    using (FileStream fs = File.OpenRead(tmp.FullName))
                    {
                        using (FileStream outFS = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
                        {
                            if (response.Compression == CommChannel.GZIP)
                            {
                                using (GZipStream gs = new GZipStream(fs, CompressionMode.Decompress))
                                {
                                    gs.CopyTo(outFS);
                                }
                            }
                            else if (response.Compression == CommChannel.DEFLATE)
                            {
                                using (DeflateStream ds = new DeflateStream(fs, CompressionMode.Decompress))
                                {
                                    ds.CopyTo(outFS);
                                }
                            }
                        }
                    }
                }
                // Delete the tmp file
                if(File.Exists(tmp.FullName))
                    File.Delete(tmp.FullName);
            }
            else if(response.Type == CommChannel.ERROR)
            {
                Console.WriteLine("Error downloading file");
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
                path = Path.Combine(localPWD, Path.GetFileName(s));
            }
            else
            {
                path = s;
            }
            return path;
        }
        /// <summary>
        /// Helper function to print out download progress bar
        /// Shows percentage based on 0-100
        /// </summary>
        /// <param name="bytesRead">Bytes read of the file</param>
        /// <param name="downloadSize">Size of the file</param>
        /// <param name="fileName">Name of file</param>
        private void DownloadStatus(long bytesRead, long downloadSize, string fileName)
        {
            // Get the current console BufferWidth
            int width = Console.BufferWidth;
            double percentComplete = Math.Round(((double)bytesRead / (double)downloadSize) * 100);
            string progressBeginning = "Downloading " + fileName + " |";
            string progressEnd = "| " + percentComplete.ToString() + "%";
            // How much screen buffer have we used so far
            int bufferRemaining = width - (progressBeginning.Length + progressEnd.Length);
            int numEquals = (int)Math.Round((percentComplete/100) * (double)bufferRemaining);
            string equalSigns = new String('=', numEquals);
            bufferRemaining -= equalSigns.Length;
            string spaces = new string(' ', bufferRemaining);
            string progress = progressBeginning + equalSigns + spaces + progressEnd;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(progress);
        }
        /// <summary>
        /// Helper function to get the network connections
        /// </summary>
        /// <param name="r">Random object to create a session id</param>
        private void Netstat(Random r)
        {
            CommHeader netstatHeader = CreateHeader(CommChannel.NETSTAT, compression, CommChannel.COMMAND, r.Next(), 0);
            channel.SendHeader(netstatHeader);
        }
        /// <summary>
        /// Helper function to get the list of processes
        /// </summary>
        /// <param name="r">Random object to create a session id</param>
        private void ProcessList(Random r)
        {
            CommHeader processListHeader = CreateHeader(CommChannel.PS, compression, CommChannel.COMMAND, r.Next(), 0);
            channel.SendHeader(processListHeader);
        }
        /// <summary>
        /// Helper function for listing of a directory.
        /// This function is called when the LS command is given
        /// </summary>
        /// <param name="commandString">
        /// Properly parsed command and arguments.
        /// Example: ls C:\Users
        /// </param>
        /// <param name="r">Random object to create a session id</param>
        private void ListDirectory(string[] commandString, Random r)
        {
            // Check for a directory argument
            if (commandString.Length > 1)
            {
                string dir = commandString[1];
                if (!isCompressed)
                {
                    int len = dir.Length;
                    CommHeader c = CreateHeader(CommChannel.LS, compression, CommChannel.COMMAND, r.Next(), len);
                    channel.SendHeader(c);
                    channel.SendBytes(Encoding.UTF8.GetBytes(dir));
                }
                else
                {
                    byte[] compressed = channel.Compress(Encoding.UTF8.GetBytes(dir));
                    int len = compressed.Length;
                    CommHeader c = CreateHeader(CommChannel.LS, compression, CommChannel.COMMAND, r.Next(), len);
                    channel.SendHeader(c);
                    channel.SendBytes(compressed);
                }
            }
            else
            {
                CommHeader c = CreateHeader(CommChannel.LS, compression, CommChannel.COMMAND, r.Next(), 0);
                channel.SendHeader(c);
            }
        }
        /// <summary>
        /// Helper function for changing directories.
        /// This function is called when the CD command is given
        /// </summary>
        /// <param name="commandString">
        /// Properly parsed command and arguments.
        /// Example: cd C:\Users
        /// </param>
        /// <param name="r">Random object to create a session id</param>
        private void ChangeDirectory(string[] commandString, Random r)
        {
            // Check for a directory argument
            if (commandString.Length > 1)
            {
                string dir = commandString[1];
                if (!isCompressed)
                {
                    int len = dir.Length;
                    CommHeader c = CreateHeader(CommChannel.CD, compression, CommChannel.COMMAND, r.Next(), len);
                    channel.SendHeader(c);
                    channel.SendBytes(Encoding.UTF8.GetBytes(dir));
                }
                else
                {
                    byte[] compressed = channel.Compress(Encoding.UTF8.GetBytes(dir));
                    int len = compressed.Length;
                    CommHeader c = CreateHeader(CommChannel.CD, compression, CommChannel.COMMAND, r.Next(), len);
                    channel.SendHeader(c);
                    channel.SendBytes(compressed);
                }
            }
            else
            {
                CommHeader c = CreateHeader(CommChannel.CD, compression, CommChannel.COMMAND, r.Next(), 0);
                channel.SendHeader(c);
            }
        }
        /// <summary>
        /// Helper function to print the present working directory.
        /// This function is called when the PWD command is given
        /// </summary>
        /// <param name="r">Random object to create a session id</param>
        private void PresentWorkingDirectory(Random r)
        {
            CommHeader lowPWDHeader = CreateHeader(CommChannel.PWD, compression, CommChannel.COMMAND, r.Next(), 0);
            channel.SendHeader(lowPWDHeader);
        }
        /// <summary>
        /// Helper function to delete the file or directory
        /// Will error if permissions issues or if the directory is not empty
        /// </summary>
        /// <param name="commandString">
        /// Properly parsed command argument
        /// Example: delete test.txt
        /// </param>
        /// <param name="r">Random object to create a session id</param>
        private void DeleteFilesysObject(string[] commandString, Random r)
        {
            if(commandString.Length > 1)
            {
                string obj = commandString[1];
                byte[] message = channel.Compress(Encoding.UTF8.GetBytes(obj));
                int len = message.Length;
                CommHeader c = CreateHeader(CommChannel.DELETE, compression, CommChannel.COMMAND, r.Next(), len);
                channel.SendHeader(c);
                channel.SendBytes(message);
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
