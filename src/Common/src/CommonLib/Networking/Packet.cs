using CommonLib.Messaging.Base;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Networking
{
    public struct Packet
    {
        public int size;
        public byte[] buffer;

        public static Packet Empty
        {
            get
            {
                return new Packet() { size = 0, buffer = null };
            }
        }
    }

    public class PacketReader
    {
        private static readonly uint PACKET_SIZE_BYTES = 4;

        private readonly Socket _socket;
        private readonly byte[] _sizeBuffer;

        public PacketReader(Socket socket)
        {
            _socket = socket;
            _sizeBuffer = new byte[PACKET_SIZE_BYTES];
        }

        public Packet GetNextPacket()
        {
            Packet res = new Packet();

            SocketError error;
            var readCount = _socket.Receive(_sizeBuffer, 0, _sizeBuffer.Length, SocketFlags.None, out error);

            if (readCount == 0)
            {
                CLog.W("Received no byte. This means the socket was closed.");
                return default(Packet);
            }

            if (!_socket.Connected)
            {
                return default(Packet);
            }
            else if (readCount != _sizeBuffer.Length)
            {
                CLog.E("Invalid number of bytes read. Expecting 4, received: {0}", readCount);
                return default(Packet);
            }
            else if (error != SocketError.Success)
            {
                CLog.E("Failed to read size of packet. Error: {0}", error);
                return default(Packet);
            }

            res.size = BitConverter.ToInt32(_sizeBuffer, 0);

            if (res.size <= 0)
            {
                CLog.E("Invalid packet size received: {0}", res.size);
                return default(Packet);
            }

            res.buffer = new byte[res.size];
            readCount = _socket.Receive(res.buffer, 0, Math.Min(res.size, ClientSocket.SOCKET_BUFFER_SIZE), SocketFlags.None, out error);

            if (readCount == 0)
            {
                CLog.W("Received no byte. This means the socket was closed.");
                return default(Packet);
            }

            int totalReadCount = readCount;
            while (totalReadCount < res.size)
            {
                var toRead = Math.Min(res.size - totalReadCount, ClientSocket.SOCKET_BUFFER_SIZE);
                readCount = _socket.Receive(res.buffer, totalReadCount, toRead, SocketFlags.None, out error);

                if (error != SocketError.Success)
                {
                    CLog.E("Failed to read data of packet. Error: {0}", error);
                    return default(Packet);
                }

                totalReadCount += readCount;
            }

            if (totalReadCount != res.size)
            {
                CLog.E("Invalid number of bytes read. Expecting {0}, received: {1}", res.size, totalReadCount);
                return default(Packet);
            }
            else if (error != SocketError.Success)
            {
                CLog.E("Failed to read data of packet. Error: {0}", error);
                return default(Packet);
            }

            return res;
        }
    }

    public class PacketWriter
    {
        private readonly Socket _socket;

        public PacketWriter(Socket socket)
        {
            _socket = socket;
        }

        public bool Write(Packet packet)
        {
            if (packet.buffer == null || packet.size != packet.buffer.Length)
            {
                CLog.E("Invalid packet to write.");
                return false;
            }

            var sizeBuffer = BitConverter.GetBytes(packet.size);
            var buffer = new byte[packet.size + sizeBuffer.Length];

            int i = 0;
            foreach (var b in sizeBuffer)
                buffer[i++] = b;

            foreach (var b in packet.buffer)
                buffer[i++] = b;

            var sentCount = _socket.Send(buffer);

            if (sentCount != buffer.Length)
            {
                CLog.E("Failed to send packet size. Expecting to send 4 bytes, actually sent: {0}", sentCount);
                return false;
            }

            return true;
        }

        public bool Write<T>(T message) where T : IMessage
        {
            var data = MessageSerializer.Serialize(message);

            Packet p;
            p.size = data.Length;
            p.buffer = data;

            return Write(p);
        }
    }
}
