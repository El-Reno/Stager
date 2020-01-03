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
        /// <summary>
        /// This method receives a message from the connection.
        /// </summary>
        /// <param name="buffer"></param>
        /*public override CommMessage ReceiveMessage()
        {
            int bytes = -1;
            byte[] buffer = new byte[CHUNK_SIZE]; // 1024
            Stream stream = tcpClient.GetStream();
            if (tcpClient.Connected)
            {
                bytes = IPAddress.NetworkToHostOrder(stream.Read(buffer, 0, buffer.Length));
            }

            // Now, get the CommHeader
            int command_compression = (int)((buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | (buffer[3] << 0));
            int data_length = (int)((buffer[4] << 24) | (buffer[5] << 16) | (buffer[6] << 8) | (buffer[7] << 0));
            CommandHeader header = new CommandHeader(command_compression, data_length, buffer);

            // Now, create the message from the buffer and send in chunks
            byte[] msg = new byte[data_length];
            if (tcpClient.Connected)
            {
                byte[] chunk;
                int total_bytes_read = 0;
                Console.WriteLine("Total bytes {0}", data_length);
                while (total_bytes_read < data_length) {
                    using (MemoryStream msgStream = new MemoryStream(msg))
                    {
                        using (BinaryWriter wr = new BinaryWriter(msgStream))
                        {
                            int bytes_remaining = data_length - total_bytes_read;
                            if (bytes_remaining < CHUNK_SIZE)
                            {
                                chunk = new byte[bytes_remaining];
                                stream.Read(chunk, 0, bytes_remaining);
                                Console.WriteLine("Bytes read {0}", total_bytes_read);
                                wr.Write(msg, total_bytes_read, bytes_remaining);
                                total_bytes_read += bytes_remaining;
                            }
                            else
                            {
                                chunk = new byte[CHUNK_SIZE];
                                stream.Read(chunk, 0, CHUNK_SIZE);
                                Console.WriteLine("Bytes read {0}", total_bytes_read);
                                
                                wr.Write(msg, total_bytes_read, CHUNK_SIZE);
                                Console.WriteLine(Encoding.UTF8.GetString(msg));
                                total_bytes_read += chunk.Length;
                            }
                        }
                    }
                }
            }

            return new CommMessage(header, msg);
        }*/

        public override void SendBytes(byte[] message)
        {
            throw new NotImplementedException();
        }

        public override void SendByte(byte b)
        {
            throw new NotImplementedException();
        }

        public override void SendInt(int i)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceiveBytes(int bytes)
        {
            throw new NotImplementedException();
        }

        public override int ReceiveInt()
        {
            throw new NotImplementedException();
        }

        public override byte ReceiveByte()
        {
            throw new NotImplementedException();
        }

        public override void SendHeader(CommandHeader header)
        {
            using (Stream s = tcpClient.GetStream())
            {
                s.Write(header.GetBytes, 0, CommandHeader.GetHeaderSize);
            }
        }

        public override CommandHeader ReceiveHeader()
        {
            byte[] header = new byte[CommandHeader.GetHeaderSize];
            using (Stream s = tcpClient.GetStream())
            {
                IPAddress.NetworkToHostOrder(s.Read(header, 0, CommandHeader.GetHeaderSize));
                int command_compression = (int)((header[0] << 24) | (header[1] << 16) | (header[2] << 8) | (header[3] << 0));
                int data_length = (int)((header[4] << 24) | (header[5] << 16) | (header[6] << 8) | (header[7] << 0));
                return new CommandHeader(command_compression, data_length, header);
            }
        }
    }
}
