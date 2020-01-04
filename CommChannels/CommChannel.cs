using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reno.Comm
{
    public abstract class CommChannel
    {
        public const int PWD = 0b00000000;
        public const int LS = 0b00000001;
        public const int CD = 0b00000010;
        public const int PS = 0b00000011;
        public const int NETSTAT = 0b00000100;
        public const int DOWNLOAD = 0b00000101;
        public const int UPLOAD = 0b00000110;
        public const int DELETE = 0b00000111;
        public const int EXIT = 0b00001000;

        public const int NONE = 0b10000000;
        public const int GZIP = 0b01110000;
        public const int DEFLATE = 0b01100000;

        public const int CHUNK_SIZE = 1024;

        public abstract void SendBytes(byte[] message);
        public abstract void SendByte(byte b);
        public abstract void SendInt(int i);
        public abstract void SendHeader(CommandHeader header);

        public abstract byte[] ReceiveBytes(int bytes);
        public abstract int ReceiveInt(BinaryReader r);
        public abstract byte ReceiveByte(BinaryReader r);
        public abstract CommandHeader ReceiveHeader();
    }
}
