﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        string prompt;
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
            while (run)
            {
                Console.Write(prompt);
                string command = Console.ReadLine();
                if(command.Equals('\t'))
                {
                    Console.WriteLine("TAB");
                }
                string[] commandString = Utility.ParseCommand(command);
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
                        continue;
                }
                if (run)
                {
                    CommHeader responseHeader = channel.ReceiveHeader();
                    //byte[] response = channel.ReceiveBytes(responseHeader.DataLength);
                    string sResponse = "";
                    if (isCompressed)
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
