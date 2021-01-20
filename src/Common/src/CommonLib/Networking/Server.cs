#if _SERVER

using CommonLib.Messaging.Base;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib.Networking
{
    public abstract class ClientConnection
    {
        protected uint _id;

        public uint ID
        {
            get
            {
                return _id;
            }
        }

        protected Socket _socket;
        protected Thread _thread;
        protected bool _running;
        protected ConnectionClosed _connectionClosed;
        protected PacketReader _reader;
        protected PacketWriter _writer;
        protected bool _closed;
        protected bool _ready;

        /// <summary>
        /// Indicates wheter the disconnection was requested by server or not. If isn't this mean client shutdown the connection or went down.
        /// </summary>
        protected bool _closeRequested;

        public bool IsActive { get => _running; }
        public bool Connected { get => _socket?.Connected ?? false; }

        public virtual void Setup(Socket socket, uint clientId, ConnectionClosed connectionClosedCB)
        {
            _socket = socket;
            _id = clientId;
            _connectionClosed = connectionClosedCB;
            _reader = new PacketReader(socket);
            _writer = new PacketWriter(socket);
        }

        public virtual void Start()
        {
            _thread = new Thread(ReceivePackets);
            _thread.Start();
            _closed = false;
            _closeRequested = false;
        }

        protected void Ready()
        {
            _ready = true;
        }

        public virtual void Stop()
        {
            _running = false;
            _closeRequested = true;
            Close();
        }

        public virtual void ReceivePackets()
        {
            CLog.D("Received connection from {0}, ID: {1}. Waiting for packets...", _socket.RemoteEndPoint.ToString(), _id);

            _running = true;
            while (_running)
            {
                try
                {
                    if (!_ready)
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    var packet = _reader.GetNextPacket();

                    if (packet.buffer == null || packet.size == 0 || packet.size != packet.buffer.Length)
                    {
                        if (_socket.Connected)
                            CLog.E("Invalid packet received. Closing connection.");

                        break;
                    }

                    Handle(packet);
                }
                catch (Exception e)
                {
                    CLog.E("Exception {0} was raised while handling packet. Catching it...", e.GetType().Name);
                    CLog.Catch(e);

                    if (!_closed)
                    {
                        Close();
                    }
                }
            }

            Close();
        }

        protected virtual void Close()
        {
            if (_closed)
                return;

            try
            {
                CLog.I("Closing connection: {0}, id: {1}, remote disconnection: {2}",
                    _socket?.RemoteEndPoint.ToString() ?? "None",
                    _id,
                    !_closeRequested);

                if (_socket.Connected)
                    _socket.Close();

                _thread.Join(50);
            }
            catch (Exception) { }

            if (_thread?.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                _thread.Abort();
            }

            _running = false;
            _connectionClosed(_id);
            _closed = true;
        }

        public virtual bool Send(Packet packet)
        {
            if (!_socket?.Connected ?? false)
                return false;

            CLog.D("Sending packet to client.");

            try
            {
                return _writer.Write(packet);
            }
            catch (Exception e)
            {
                CLog.E("Failed to send packet: {0}", new RawMessage(packet.buffer).MsgType);
                CLog.Catch(e);
                return false;
            }
        }

        public virtual bool Send<T>(T packet) where T : IMessage
        {
            if (!_socket?.Connected ?? false)
                return false;

            CLog.D("Sending message {0} to client.", packet.MsgType);

            try
            {
                return _writer.Write(packet);
            }
            catch (Exception e)
            {
                CLog.E("Failed to send packet: {0}", packet.MsgType);
                CLog.Catch(e);
                return false;
            }
        }

        public abstract void Handle(Packet packet);
    }

    public delegate void ConnectionClosed(uint clientId);

    public class ServerSocket<T> where T : ClientConnection, new()
    {
        public delegate void ClientConnectedEvent(T client);

        private readonly int _listenPort;
        private readonly string _listenAddr;
        private readonly IPEndPoint _listenEndPoint;
        private readonly int _listenBacklog;

        private Socket _serverSocket;
        private bool _running;
        private List<T> _clients;
        private uint _lastClientId;

        private List<uint> _closedClients;

        protected int _maxConnections;
        protected int _connectionCount;

        private event ClientConnectedEvent _clientConnectedEvent;
        public event ClientConnectedEvent OnClientConnected
        {
            add
            {
                _clientConnectedEvent += value;
            }
            remove
            {
                _clientConnectedEvent -= value;
            }
        }

        public ServerSocket(string listenAddress, int listenPort, int backlogSize = 100)
        {
            _clients = new List<T>();
            _closedClients = new List<uint>();

            _listenAddr = listenAddress;
            _listenPort = listenPort;
            _listenBacklog = backlogSize;

            _listenEndPoint = new IPEndPoint(IPAddress.Parse(listenAddress), listenPort);
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _maxConnections = int.MaxValue;
        }

        public void Start()
        {
            _running = true;
            _serverSocket.Bind(_listenEndPoint);
            _serverSocket.Listen(_listenBacklog);

            CLog.I("Networking server started on {0}({1}) ", _listenAddr, _listenPort);

            try
            {
                while (_running)
                {
                    lock (_closedClients)
                    {
                        if (_closedClients.Count > 0)
                        {
                            CLog.D("Removing closed connections...");

                            lock (_clients)
                            {
                                _clients.RemoveAll((c) => _closedClients.Contains(c.ID));
                                _connectionCount -= _closedClients.Count;
                                _closedClients.Clear();
                            }
                        }
                    }

                    CLog.D("Waiting for incomming client connection.");

                    if (_connectionCount < _maxConnections)
                    {
                        var incomming = _serverSocket.Accept();
                        incomming.SendBufferSize = ClientSocket.SOCKET_BUFFER_SIZE;
                        incomming.ReceiveBufferSize = ClientSocket.SOCKET_BUFFER_SIZE;
                        incomming.NoDelay = true;

                        var client = new T();

                        lock (_clients)
                        {
                            _clients.Add(client);
                            _connectionCount++;
                        }

                        client.Setup(incomming, ++_lastClientId, OnClientDisconnect);
                        client.Start();

                        _clientConnectedEvent?.Invoke(client);
                    }
                    else
                    {
                        CLog.W("Max connections reached! Not more connections will be accepted on server socket: {0}", _serverSocket.LocalEndPoint);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (SocketException ex)
            {
                CLog.Catch(ex);
            }

        }

        public void Stop()
        {
            _running = false;

            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    client.Stop();
                }

                _clients.Clear();
            }

            _serverSocket.Close();
        }

        public void Disconnect(uint clientId)
        {
            if (!_running)
                return;

            lock (_clients)
            {
                _clients.Find(c => c.ID == clientId)?.Stop();
            }
        }

        public void OnClientDisconnect(uint clientId)
        {
            lock (_closedClients)
            {
                _closedClients.Add(clientId);
            }
        }
    }
}

#endif