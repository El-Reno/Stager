﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Reno.Comm
{
    public class CommandHeader
    {
        const int HEADER_LENGTH = sizeof(int) * 3;
        int command = -1;
        int compression = -1;
        int data_length = -1;
        byte[] header;
        /// <summary>
        /// Constructor for the Command header.
        /// </summary>
        /// <param name="command_compression">An integer holding the command and compression types</param>
        /// <param name="data_length">Length of the data attached to the datagram</param>
        public CommandHeader(int command_compression, int data_length, byte[] header)
        {
            command = command_compression & 0x000F;
            compression = command_compression & 0x00F0;
            this.data_length = data_length;
            this.header = header;
        }

        public static int GetHeaderSize
        {
            get
            {
                return HEADER_LENGTH;
            }
        }
        public byte[] GetBytes
        {
            get
            {
                return header;
            }
        }
        /// <summary>
        /// Returns the integer value of the command
        /// </summary>
        public int Command
        {
            get
            {
                return command;
            }
        }
        /// <summary>
        /// Returns the integer value of the compression algorithm
        /// </summary>
        public int Compression
        {
            get
            {
                return compression;
            }
        }
    }
}
