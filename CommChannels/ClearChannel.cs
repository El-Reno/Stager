using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;

namespace Reno.Comm
{
    public class ClearChannel : CommChannel
    {
        TcpClient tcpClient;
        string destination, compression;
        int port;

        /// <summary>
        /// Creates an object meant to handle unencrypted communications to an endpoint.
        /// The constructor creates and connects to the endpoint using a TcpClient class.
        /// </summary>
        /// <param name="destination">The endpoint IP address or hostname</param>
        /// <param name="port">Port to connect to</param>
        /// <param name="compression">Compression algorithm - NONE, GZIP, or DEFLATE</param>
        public ClearChannel(string destination, int port, string compression)
        {
            this.destination = destination;
            this.compression = compression;
            this.port = port;
            try
            {
                tcpClient = new TcpClient(destination, port);
            }
            catch(ArgumentNullException argNull)
            {

            }
            catch(ArgumentOutOfRangeException argRange)
            {

            }
            catch(SocketException e)
            {

            }
            
        }
        public override void SendMessage()
        {

        }
        /// <summary>
        /// This method receives a message from the connection.
        /// </summary>
        /// <param name="buffer"></param>
        public override byte[] ReceiveMessage()
        {
            int bytes = -1;
            byte[] buffer = new byte[CHUNK_SIZE]; // 1024
            byte command;
            byte compression;
            byte[] length = new byte[2];
            Stream stream = tcpClient.GetStream();
            if (tcpClient.Connected)
            {
                // Read the command
                int tmp = stream.ReadByte();
                Console.WriteLine("Byte read: " + tmp);
                do
                {

                    bytes = stream.Read(buffer, 0, buffer.Length);

                } while (bytes != 0);
            }

            return buffer;
        }
    }
}
