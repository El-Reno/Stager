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
            /*this.destination = destination;
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

            }*/
            
        }
        /// <summary>
        /// Creates an object meant to handle unencrypted communications to an endpoint.
        /// The constructor accepts a TcpClient to handle communications
        /// </summary>
        /// <param name="client">TcpClient for client/server communication</param>
        public ClearChannel(TcpClient client)
        {
            tcpClient = client;
        }

        public override void SendBytes(BinaryWriter w, byte[] message)
        {
            w.Write(message);
        }

        public override void SendByte(BinaryWriter w, byte b)
        {
            throw new NotImplementedException();
        }

        public override void SendInt(BinaryWriter w, int i)
        {
            throw new NotImplementedException();
        }

        public override byte[] ReceiveBytes(BinaryReader r, int bytes)
        {
            byte[] buffer = new byte[bytes];
            int bytesRead = 0;
            using (var m = new MemoryStream(buffer))
            {
                using (var b = new BinaryWriter(m))
                {
                    while(bytesRead < bytes)
                    {
                        b.Write(ReceiveByte(r));
                        bytesRead++;
                    }
                    return buffer;
                }
            }
        }

        public override int ReceiveInt(BinaryReader r)
        {
            return IPAddress.NetworkToHostOrder(r.ReadInt32());
        }

        public override byte ReceiveByte(BinaryReader r)
        {
            return (byte)IPAddress.NetworkToHostOrder((int)r.ReadByte());
        }
        /// <summary>
        /// Sends the CommandHander to the connected server in network byte order
        /// </summary>
        /// <param name="header">CommandHeader to transmit</param>
        public override void SendHeader(BinaryWriter w, CommHeader header)
        {
            int combined;
            // Combine byte fields to int
            combined = (header.Command << 24) | (header.Compression << 16) | (header.Type << 8) | (header.Reserved << 0);
            w.Write(IPAddress.HostToNetworkOrder(combined));
            w.Write(IPAddress.HostToNetworkOrder(header.Id));
            w.Write(IPAddress.HostToNetworkOrder(header.DataLength));
        }
        /// <summary>
        /// Receives a CommandHeader from the connected server
        /// </summary>
        /// <returns>CommandHeader from the server to initiate a command</returns>
        public override CommHeader ReceiveHeader(BinaryReader r)
        {
            //using (Stream s = tcpClient.GetStream())
            int combined = 0;
            byte command = 0;
            byte compression = 0;
            byte t = 0;
            byte reserved = 0;
            int id = 0;
            int data_length = 0;
            
            combined = ReceiveInt(r);
            // Split the combined int into the right fields
            command = (byte)((combined >> 24) & 0xFF);
            compression = (byte)((combined >> 16) & 0xFF);
            t = (byte)((combined >> 8) & 0xFF);
            reserved = (byte)(combined & 0xFF);
            // Read the remainder of the header
            id = ReceiveInt(r); 
            data_length = ReceiveInt(r);

            return new CommHeader(command, compression, t, reserved, id, data_length);
        }
    }
}
