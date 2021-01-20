using CommonLib.Messaging.Base;
using CommonLib.Messaging.Common;
using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib.Networking
{
    public abstract class ClientSocket
    {
        public const int SOCKET_BUFFER_SIZE = 15000;

        /// <summary>
        /// Server Port.
        /// </summary>
        private readonly int _serverPort;

        /// <summary>
        /// Server IP.
        /// </summary>
        private readonly string _serverHost;

        private readonly IPEndPoint _serverEndPoint;

        /// <summary>
        /// Pending Packets.
        /// </summary>
        private readonly List<Packet> _pendingPackets;

        private Socket _socket;
        private Thread _thread;

        private bool _running;
        private bool _closed;

        private PacketReader _reader;
        private PacketWriter _writer;

        private object _reconnectLock;

#if _DEBUG
        private Random _randLag = new Random((int)DateTime.Now.Ticks);
        private uint _latencySimulation;
        public uint LatencySimulation
        {
            get
            {
                return _latencySimulation;
            }
            set
            {
                _latencySimulation = value;
            }
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverHost"></param>
        /// <param name="serverPort"></param>
        public ClientSocket(string serverHost, int serverPort)
        {
            _closed = false;
            _pendingPackets = new List<Packet>();

            //var serverIp = Dns.GetHostEntry(serverHost).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            var serverIp = System.Net.Dns.GetHostAddresses(serverHost)[0];

            if (serverIp == null)
            {
                CLog.F("Failed to resolve address: {0}.", serverHost);
                return;
            }

            _reconnectLock = new object();

            _serverHost = serverHost;
            _serverPort = serverPort;
            _serverEndPoint = new IPEndPoint(serverIp, serverPort);
        }

        /// <summary>
        /// Start connection with Client.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            _closed = false;

            try
            {
                _thread = new Thread(ReceivePackets);
                _thread.Start();
            }
            catch (Exception e)
            {
                CLog.Catch(e);

                return false;
            }

            OnStart();

            return true;
        }

        protected virtual void OnStart()
        {
        }

#if _DEBUG
        private void SimulateLatency()
        {
            if (_latencySimulation > 0)
            {
                var lagVar = _latencySimulation * 0.3f;
                var latency = _randLag.Next((int)(_latencySimulation - lagVar), (int)(_latencySimulation + lagVar));

                CLog.D("Simulating latency of {0}ms.", latency);
                Thread.Sleep(latency);
            }
        }
#endif

        public virtual void ReceivePackets()
        {
            _running = true;
            Reconnect(true);

            CLog.D("Waiting for server packets...");

            while (_running)
            {
                try
                {
                    var packet = _reader.GetNextPacket();

                    if (packet.buffer == null || packet.size == 0 || packet.size != packet.buffer.Length)
                    {
                        if (_socket.Connected)
                        {
                            CLog.E("Invalid packet received. Closing connection.");
                        }
                        else if (_running)
                        {
                            Reconnect();
                            continue;
                        }

                        break;
                    }

#if _DEBUG
                    SimulateLatency();
#endif

                    Handle(packet);
                }
                catch (SocketException se)
                {
                    if (!_socket.Connected && _running)
                    {
                        Reconnect();
                        continue;
                    }
                    else if (_closed || se.GetType() == typeof(ThreadAbortException))
                    {
                        return;
                    }
                    else
                    {
                        CLog.Catch(se);
                        break;
                    }
                }
                catch (Exception e)
                {
                    CLog.E("Exception {0} was raised while handling packet. Catching it...", e.GetType().Name);
                    CLog.Catch(e);
                }
            }

            if (!_closed)
                Close();
        }

        /// <summary>
        /// This method is called once, when the socket is connected for the first time
        /// </summary>
        protected virtual void OnConnect()
        {

        }

        /// <summary>
        /// This method is called whenever the socket reconnects to the server.
        /// </summary>
        protected virtual void OnReconnect()
        {

        }

        /// <summary>
        /// This method is called whenever the socket loses the connection. It is only called if there is at least one successfull connection.
        /// </summary>
        /// <returns>Returns true if it should try to reconnect. False otherwise</returns>
        protected virtual bool OnDisconnect()
        {
            return true;
        }

        private void Reconnect(bool firstTime = false)
        {
            if (!firstTime && !OnDisconnect())
            {
                Stop();
                return;
            }

            while (_running)
            {
                try
                {
                    lock (_reconnectLock)
                    {
                        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                        {
                            NoDelay = true,
                            SendBufferSize = SOCKET_BUFFER_SIZE,
                            ReceiveBufferSize = SOCKET_BUFFER_SIZE,
                        };

                        _socket.Connect(_serverEndPoint);

                        if (!_socket.Connected)
                            continue;
                        else
                        {
                            if (firstTime)
                            {
                                OnConnect();
                            }
                            else
                            {
                                OnDisconnect();
                            }
                        }

                        CLog.W("Connected to {0}", _serverEndPoint);

                        _reader = new PacketReader(_socket);
                        _writer = new PacketWriter(_socket);

                        foreach (var p in _pendingPackets)
                        {
                            _writer.Write(p);
                        }

                        // Clear all pending packets.
                        _pendingPackets.Clear();

                        break;
                    }
                }
                catch (Exception e)
                {
                    CLog.W("Trying to connect to {0}. Error: {1}", _serverEndPoint, e.Message);
                    Thread.Sleep(1000);

                    continue;
                }
            }
        }

        /// <summary>
        /// Stop connection with Client.
        /// </summary>
        public void Stop()
        {
            if (_closed) return;

            _running = false;
            _closed = true;

            // Close connection.
            Close();
        }

        /// <summary>
        /// Close connection with Client.
        /// </summary>
        protected virtual void Close()
        {
            CLog.D("Closing connection...");

            if (_socket != null)
                _socket.Close();

            try
            {
                _thread.Join(200);
            }
            catch (Exception) { }

            if (_thread != null)
                _thread.Abort();
        }

        /// <summary>
        /// Send packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public virtual bool Send(Packet packet)
        {
            lock (_reconnectLock)
            {
                if (_socket == null || _socket.Connected == false)
                {
                    _pendingPackets.Add(packet);
                    return true;
                }
                else
                {
                    return _writer.Write(packet);
                }
            }
        }

        /// <summary>
        /// Send packet.
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public virtual bool Send<T>(T packet) where T : IMessage
        {
            lock (_reconnectLock)
            {
                if (_socket == null || _socket.Connected == false)
                {
                    var buffer = MessageSerializer.Serialize<T>(packet);

                    var pkt = new Packet()
                    {
                        buffer = buffer,
                        size = buffer.Length

                    };


                    _pendingPackets.Add(pkt);
                    return true;
                }
                else
                {
                    return _writer.Write(packet);
                }
            }
        }

        public abstract void Handle(Packet packet);
    }
}
