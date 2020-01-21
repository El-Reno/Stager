using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;

namespace Reno.Comm
{
    public class ClearChannel : CommChannel
    {
        TcpClient tcpClient;
        Stream stream;
        BinaryReader br;
        BinaryWriter bw;
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
            this.compression = compression.ToUpper();
            this.port = port;
            try
            {
                tcpClient = new TcpClient(destination, port);
                stream = tcpClient.GetStream();
                br = new BinaryReader(stream);
                bw = new BinaryWriter(stream);
            }
            catch(SocketException e)
            {
                Console.WriteLine("[-] Error opening socket: {0}", e.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("[-] Error occurred in ClearChannel constructor: {0}", ex.Message);
            }
            
        }
        /// <summary>
        /// Creates an object meant to handle unencrypted communications to an endpoint.
        /// The constructor accepts a TcpClient to handle communications
        /// </summary>
        /// <param name="client">TcpClient for client/server communication</param>
        public ClearChannel(TcpClient client, string compression)
        {
            try
            {
                tcpClient = client;
                this.compression = compression.ToUpper();
                stream = client.GetStream();
                br = new BinaryReader(stream);
                bw = new BinaryWriter(stream);
            }
            catch (SocketException e)
            {
                Console.WriteLine("[-] Error opening socket: {0}", e.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] Error occurred in ClearChannel constructor: {0}", ex.Message);
            }
        }
        /// <summary>
        /// Sends bytes over the tcp connection
        /// </summary>
        /// <param name="message">Bytes to send</param>
        public override void SendBytes(byte[] message)
        {
            if(bw != null)
                bw.Write(message);
        }
        /// <summary>
        /// Sends the byte over the tcp connection
        /// </summary>
        /// <param name="b">Byte to send</param>
        public override void SendByte(byte b)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Sends the integer over the tcp connection
        /// </summary>
        /// <param name="i">Integer to send</param>
        public override void SendInt(int i)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Sends the CommandHander to the connected server in network byte order
        /// </summary>
        /// <param name="header">CommandHeader to transmit</param>
        public override void SendHeader(CommHeader header)
        {
            int combined;
            // Combine byte fields to int
            combined = (header.Command << 24) | (header.Compression << 16) | (header.Type << 8) | (header.Reserved << 0);
            if (bw != null)
            {
                bw.Write(IPAddress.HostToNetworkOrder(combined));
                bw.Write(IPAddress.HostToNetworkOrder(header.Id));
                bw.Write(IPAddress.HostToNetworkOrder(header.DataLength));
            }
        }
        /// <summary>
        /// Receives an amount of bytes from the tcp connection
        /// </summary>
        /// <param name="bytes">Amount of bytes to receive</param>
        /// <returns>Byte array of size bytes</returns>
        public override byte[] ReceiveBytes(int bytes)
        {
            byte[] buffer = new byte[bytes];
            using (var m = new MemoryStream(buffer))
            {
                // Read the transmission into the buffer array
                using (var b = new BinaryWriter(m))
                {
                    if(br != null)
                        b.Write(br.ReadBytes(bytes));
                }
            }
            return buffer;
        }
        /// <summary>
        /// Receive an integer from the tcp connection
        /// </summary>
        /// <returns>Integer in host byte order</returns>
        public override int ReceiveInt()
        {
            int readInt = 0;
            if(br != null)
                readInt = IPAddress.NetworkToHostOrder(br.ReadInt32());
            return readInt;
        }
        /// <summary>
        /// Receive a byte from the tcp connection
        /// </summary>
        /// <returns>Byte from the tcp connection</returns>
        public override byte ReceiveByte()
        {
            byte readByte = 0;
            if(br != null)
                br.ReadByte();
            return readByte;
        }
        /// <summary>
        /// Receives a CommandHeader from the connected server
        /// </summary>
        /// <returns>CommandHeader from the server to initiate a command</returns>
        public override CommHeader ReceiveHeader()
        {
            //using (Stream s = tcpClient.GetStream())
            int combined = 0;
            byte command = 0;
            byte compression = 0;
            byte t = 0;
            byte reserved = 0;
            int id = 0;
            int data_length = 0;
            
            combined = ReceiveInt();
            // Split the combined int into the right fields
            command = (byte)((combined >> 24) & 0xFF);
            compression = (byte)((combined >> 16) & 0xFF);
            t = (byte)((combined >> 8) & 0xFF);
            reserved = (byte)(combined & 0xFF);
            // Read the remainder of the header
            id = ReceiveInt(); 
            data_length = ReceiveInt();

            return new CommHeader(command, compression, t, reserved, id, data_length);
        }
        /// <summary>
        /// Compresses a given message with the compression algorithm chosen
        /// </summary>
        /// <param name="message">The message string as a byte array</param>
        /// <returns>Compressed byte array</returns>
        public override byte[] Compress(byte[] message)
        {
            int len = message.Length;
            using(var m = new MemoryStream())
            {
                switch (compression)
                {
                    case "DEFLATE":
                        using (var d = new DeflateStream(m, CompressionLevel.Optimal))
                        {
                            d.Write(message, 0, len);
                        }
                        break;
                    case "GZIP":
                        using (var d = new GZipStream(m, CompressionLevel.Optimal))
                        {
                            d.Write(message, 0, len);
                        }
                        break;
                    case "NONE":
                        return message;
                }
                byte[] buffer = m.ToArray();
                return buffer;
            } 
        }
        /// <summary>
        /// Decompresses the compressed byte array
        /// </summary>
        /// <param name="message">Byte array with compressed message</param>
        /// <returns>Decompressed byte array</returns>
        public override byte[] Decompress(byte[] message)
        {
            using (var m = new MemoryStream())  // Empty MemoryStream to write decompressed data to
            {
                using (var msg = new MemoryStream(message))
                {
                    switch (compression)
                    {
                        case "DEFLATE":
                            using (var d = new DeflateStream(msg, CompressionMode.Decompress))
                            {
                                d.CopyTo(m);
                            }
                            break;
                        case "GZIP":
                            using (var d = new GZipStream(msg, CompressionMode.Decompress))
                            {
                                d.CopyTo(m);
                            }
                            break;
                        case "NONE":
                            return message;
                    }
                }
                return m.ToArray();
            }           
        }
        /// <summary>
        /// Closes the channel
        /// </summary>
        public override void Close()
        {
            try
            {
                if (bw != null && br != null && stream != null && tcpClient != null)
                {
                    bw.Close();
                    br.Close();
                    stream.Close();
                    tcpClient.Close();
                }
            }
            catch(IOException e)
            {
                Console.WriteLine("[-] Error closing connection: {0}", e.Message);
            }
        }
        /// <summary>
        /// Returns true if the connection is open
        /// </summary>
        /// <returns></returns>
        public override bool IsOpen()
        {
            if (tcpClient == null)
                return false;
            return tcpClient.Connected;
        }
        /// <summary>
        /// Returns the compressions
        /// </summary>
        /// <returns>Compression in byte representation</returns>
        public override byte Compression()
        {
            byte comp;
            switch (compression)
            {
                case "GZIP":
                    comp = CommChannel.GZIP;
                    break;
                case "DEFLATE":
                    comp = CommChannel.DEFLATE;
                    break;
                case "NONE":
                    comp = CommChannel.NONE;
                    break;
                default:
                    comp = CommChannel.NONE;
                    break;
            }
            return comp;
        }
    }
}
