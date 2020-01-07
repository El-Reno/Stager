using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reno.Comm
{
    public class CommHeader
    {
        const int HEADER_LENGTH = sizeof(byte) * 4 + sizeof(int) * 2;
        byte command;
        byte compression;
        byte hType;
        byte reserved;
        int id;
        int data_length;  
        /// <summary>
        /// Constructor for the Command header.
        /// </summary>
        /// <param name="command_compression">An integer holding the command and compression types</param>
        /// <param name="data_length">Length of the data attached to the datagram</param>
        public CommHeader(byte command, byte compression, byte hType, byte reserved, int id, int data_length)
        {
            this.command = command;
            this.id = id;
            this.compression = compression;
            this.hType = hType;
            this.data_length = data_length;
            this.reserved = reserved;
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
                byte[] header = new byte[HEADER_LENGTH];
                using (var memStream = new MemoryStream(header))
                {
                    using (var wr = new BinaryWriter(memStream))
                    {
                        wr.Write(command);
                        wr.Write(id);
                        wr.Write(compression);
                        wr.Write(hType);
                        wr.Write(data_length);
                        wr.Write(reserved);
                    }
                }
                return header;
            }
        }
        /// <summary>
        /// Returns the integer value of the command
        /// </summary>
        public byte Command
        {
            get
            {
                return command;
            }
        }
        public int Id
        {
            get
            {
                return id;
            }
        }
        /// <summary>
        /// Returns the integer value of the compression algorithm
        /// </summary>
        public byte Compression
        {
            get
            {
                return compression;
            }
        }

        public byte Type
        {
            get
            {
                return hType;
            }
        }
        public int DataLength
        {
            get
            {
                return data_length;
            }
        }

        public int Reserved
        {
            get
            {
                return reserved;
            }
        }
    }
}
