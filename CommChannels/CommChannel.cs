using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reno.Comm
{
    public abstract class CommChannel
    {
        // Commands
        public const byte PWD = 0b00000000;
        public const byte LS = 0b00000001;
        public const byte CD = 0b00000010;
        public const byte PS = 0b00000011;
        public const byte NETSTAT = 0b00000100;
        public const byte DOWNLOAD = 0b00000101;
        public const byte UPLOAD = 0b00000110;
        public const byte DELETE = 0b00000111;
        public const byte EXIT = 0b00001000;
        public const byte EXECUTE = 0b00001001;
        // Compression 
        public const byte NONE = 0b10000000;
        public const byte GZIP = 0b01110000;
        public const byte DEFLATE = 0b01100000;
        // Message type
        public const byte COMMAND = 0b00000000;
        public const byte RESPONSE = 0b00000001;
        public const byte ERROR = 0b00000010;

        public const int CHUNK_SIZE = 1024;

        public abstract void SendBytes(byte[] message);
        public abstract void SendByte(byte b);
        public abstract void SendInt(int i);
        public abstract void SendHeader(CommHeader header);

        public abstract byte[] ReceiveBytes(int bytes);
        public abstract int ReceiveInt();
        public abstract byte ReceiveByte();
        public abstract byte[] Compress(byte[] message);
        public abstract byte[] Decompress(byte[] message);
        public abstract CommHeader ReceiveHeader();
        public abstract void Close();
        public abstract bool IsOpen();
        public abstract byte Compression();
    }
}
