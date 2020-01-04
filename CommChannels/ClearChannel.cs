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
        /// Creates an object meant to handle unencrypted communications to an endpoint.
        /// The constructor accepts a TcpClient to handle communications
        /// </summary>
        /// <param name="client">TcpClient for client/server communication</param>
        public ClearChannel(TcpClient client)
        {
            tcpClient = client;
        }

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

        public override int ReceiveInt(BinaryReader r)
        {
            return IPAddress.NetworkToHostOrder(r.ReadInt32());
        }

        public override byte ReceiveByte(BinaryReader r)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Sends the CommandHander to the connected server in network byte order
        /// </summary>
        /// <param name="header">CommandHeader to transmit</param>
        public override void SendHeader(CommandHeader header)
        {
            using (Stream s = tcpClient.GetStream())
            {
                using (BinaryWriter w = new BinaryWriter(s))
                {
                    int command_commpression = header.Command | header.Compression;     // Combine the two fields
                    w.Write(IPAddress.HostToNetworkOrder(command_commpression));
                    w.Write(IPAddress.HostToNetworkOrder(header.DataLength));
                }              
            }
        }
        /// <summary>
        /// Receives a CommandHeader from the connected server
        /// </summary>
        /// <returns>CommandHeader from the server to initiate a command</returns>
        public override CommandHeader ReceiveHeader()
        {
            using (Stream s = tcpClient.GetStream())
            {
                using (BinaryReader r = new BinaryReader(s))
                {
                    int command_compression = ReceiveInt(r);
                    int data_length = ReceiveInt(r);
                    Console.WriteLine("Data length {0}", data_length);
                    return new CommandHeader(command_compression, data_length);
                }
            }
        }
    }
}
