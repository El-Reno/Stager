using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reno.Comm
{
    public abstract class CommChannel
    {
        public enum Command
        {
            pwd = 0x0000,
            ls = 0x0001,
            cd = 0x0010,
            ps = 0x0011,
            netstat = 0x0100,
            download = 0x0101,
            upload = 0x0110,
            delete = 0x0111,
            exit = 0x1000
        }
        public const int CHUNK_SIZE = 1024;

        public abstract void SendMessage();

        public abstract byte[] ReceiveMessage();
    }
}
