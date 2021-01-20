using CommonLib.Messaging.DB;
using CommonLib.Networking;
using CommonLib.Server;
using CommonLib.Util;
using LobbyServer.Logic;
using LobbyServer.Logic.OAuth;
using LobbyServer.Logic.Resource;
using LobbyServer.Logic.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace LobbyServer.Server
{
    internal partial class AppServer : GameLoopServer
    {
        private static readonly uint TICKS_PER_SECOND = 60;

        enum StatusInfo
        {
            PlayerCount,
            RoomCount,
            RoomPlaying,

            MAX, //Don't change this!
        }

        private Thread _rsThread;
        private ServerSocket<RoomServerSession> _rsServer;

        private Thread _socketThread;
        private ServerSocket<ClientSession> _socketServer;

        private readonly List<WeakReference<ClientSession>> _connectedSessions;
        private Timer _clearSessionsTimer;

        private DatabaseClient _dbClient;
        public DatabaseClient DBClient { get => _dbClient; }

        private readonly RoomController _roomController;
        public RoomController RoomController { get => _roomController; }

        private readonly PlayerController _playerController;
        public PlayerController PlayerController { get => _playerController; }

        private readonly RoomServerController _roomServerController;
        public RoomServerController RoomServerController => _roomServerController;

        private readonly ResourceController _resourceController;
        public ResourceController ResourceController => _resourceController;

        private object _readyLock;

        public AppServer(uint instanceId) : base(instanceId, Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), TICKS_PER_SECOND)
        {
            _readyLock = new object();
            _connectedSessions = new List<WeakReference<ClientSession>>();
            _roomController = new RoomController(this);
            _playerController = new PlayerController(this);
            _roomServerController = new RoomServerController(this);
            _resourceController = new ResourceController();
        }

        public override bool Init()
        {
            if (!base.Init())
                return false;

            for(var i = 0; i < (int)StatusInfo.MAX; i++)
            {
                AddStatusInfo();
            }

            CLog.D("   _      ____  ____  ______     _______ ______ _______      ________ _____  ");
            CLog.D("  | |    / __ \\|  _ \\|  _ \\ \\   / / ____|  ____|  __ \\ \\    / /  ____|  __ \\ ");
            CLog.D("  | |   | |  | | |_) | |_) \\ \\_/ / (___ | |__  | |__) \\ \\  / /| |__  | |__) |");
            CLog.D("  | |   | |  | |  _ <|  _ < \\   / \\___ \\|  __| |  _  / \\ \\/ / |  __| |  _  / ");
            CLog.D("  | |___| |__| | |_) | |_) | | |  ____) | |____| | \\ \\  \\  /  | |____| | \\ \\ ");
            CLog.D("  |______\\____/|____/|____/  |_| |_____/|______|_|  \\_\\  \\/   |______|_|  \\_\\");
            CLog.I("");

            string welcomeMsg = string.Format("Server is initialized at: {0:HH:mm:ss tt}", DateTime.Now);
            CLog.I(welcomeMsg);
            CLog.I("");

            base.ShowConfigInfo();

            SetupNetworking();

            InitResources();

            // If Telegram is enabled, let to setup.
            if (ResourceController.TelegramEnabled)
            {
                SetupTelegram("LobbyServer", ResourceController.TelegramToken, ResourceController.TelegramGroupID, (CLogType)ResourceController.TelegramLogFilter);
                _telegramHelper.SendMessage(welcomeMsg);
            }

            StartInternalServer();
            RequestStartupInfo();
            PublishServer();

            CLog.filter = (CLogType)_resourceController.LogFilter;

            return true;
        }

        private void RequestStartupInfo()
        {
            DBClient.Send(new DL_STARTUP_INFO_REQ());
        }

        private void SetupNetworking()
        {
            string dbServerIP = GetConfig("dbServerAddress", "127.0.0.1");
            int dbServerPort = int.Parse(GetConfig("dbServerPort", "11510"));

            _dbClient = new DatabaseClient(this, dbServerIP, dbServerPort);
            _dbClient.Start();
        }

        private void InitResources()
        {
            XElement xml = null;

            try
            {
                xml = XElement.Load(@"LobbyServer.xml");
            }
            catch(Exception)
            {
                throw new Exception(string.Format("Failed to read {0}.", "LobbyServer.xml"));
            }

            // Path of resource folder.
            _resourceController.PathResource = XMLHelper.GetSafeAttributeStr(xml, "Resource", "Path");

            // Log Filter.
            _resourceController.LogFilter = (int)XMLHelper.GetSafeAttribute(xml, "Log", "Filter");

            // Telegram information.
            _resourceController.TelegramEnabled = (bool)XMLHelper.GetSafeAttribute(xml, "Telegram", "Enabled");
            if(_resourceController.TelegramEnabled)
            {
                _resourceController.TelegramToken = XMLHelper.GetSafeAttributeStr(xml, "Telegram", "Token");
                _resourceController.TelegramGroupID = (int)XMLHelper.GetSafeAttribute(xml, "Telegram", "GroupID");
                _resourceController.TelegramLogFilter = (int)XMLHelper.GetSafeAttribute(xml, "Telegram", "LogFilter");
            }

            _resourceController.Initialization();
        }

        public string GetGlobalConfig(string name, string defaultValue)
        {
            return GetConfig(name, defaultValue);
        }

        protected override void OnStart()
        {
            var oneMin = TimeSpan.FromMinutes(1).Milliseconds;

            _clearSessionsTimer = new Timer(s => RemovedEmptySessionRef(), null, oneMin, oneMin);
        }

        protected override void OnClose()
        {
            base.OnClose();

            _clearSessionsTimer?.Dispose();
            _socketServer?.Stop();
            _rsServer?.Stop();
            _dbClient?.Stop();
            _roomServerController?.Clear();
        }

        private void StartInternalServer()
        {
            string listenAddr = GetConfig("rmListenAddress", "0.0.0.0");
            int port = int.Parse(GetConfig("rmListenPort", "11511"));

            _rsServer = new ServerSocket<RoomServerSession>(listenAddr, port);
            _rsServer.OnClientConnected += OnRoomServerSessionConnected;

            _rsThread = new Thread(_rsServer.Start);
            _rsThread.Start();
        }

        private void PublishServer()
        {
            string listenAddr = GetConfig("listenAddress", "0.0.0.0");
            int port = int.Parse(GetConfig("listenPort", "9876"));

            _socketServer = new ServerSocket<ClientSession>(listenAddr, port);
            _socketServer.OnClientConnected += OnClientSessionConnected;

            _socketThread = new Thread(_socketServer.Start);
            _socketThread.Start();

        }

        private void OnClientSessionConnected(ClientSession session)
        {
            session.Setup(this);

            lock (_connectedSessions)
            {
                _connectedSessions.Add(new WeakReference<ClientSession>(session));
            }
        }

        private void OnRoomServerSessionConnected(RoomServerSession session)
        {
            _roomServerController.OnConnect(session);
        }

        public bool IsConnected(string login)
        {
            lock (_connectedSessions)
            {
                return _connectedSessions.Exists((wref) => (wref.TryGetTarget(out var s)) ? s.Login == login : false);
            }
        }

        private void RemovedEmptySessionRef()
        {
            lock (_connectedSessions)
            {
                _connectedSessions.RemoveAll((wref) => (wref.TryGetTarget(out var s)) ? !s.IsActive : true);
            }
        }

        internal ClientSession FindSession(uint id)
        {
            WeakReference<ClientSession> wref;

            lock (_connectedSessions)
            {
                wref = _connectedSessions.Find((wr) => (wr.TryGetTarget(out var s)) ? s.ID == id && s.IsActive : false);
            }

            if (wref == null)
                return null;

            return (wref.TryGetTarget(out var session)) ? session : null;
        }

        internal ClientSession FindSession(string login)
        {
            WeakReference<ClientSession> wref;

            lock (_connectedSessions)
            {
                wref = _connectedSessions.Find((wr) => (wr.TryGetTarget(out var s)) ? s.Login == login && s.IsActive : false);
            }

            if (wref == null)
                return null;

            return (wref.TryGetTarget(out var session)) ? session : null;
        }

        internal ClientSession FindSessionByToken(string token)
        {
            WeakReference<ClientSession> wref;

            lock (_connectedSessions)
            {
                wref = _connectedSessions.Find((wr) => (wr.TryGetTarget(out var s)) ? s.Token == token && s.IsActive : false);
            }

            if (wref == null)
                return null;

            return (wref.TryGetTarget(out var session)) ? session : null;
        }

        public override void Tick(float delta)
        {
            UpdateStatusInfo(delta);
        }

        private float _updateStatusInfoTimeout;
        private void UpdateStatusInfo(float delta)
        {
            if (_updateStatusInfoTimeout > 0)
            {
                _updateStatusInfoTimeout -= delta;
                return;
            }

            _updateStatusInfoTimeout = 1f; //Update every 1 second

            SetStatusInfo((int)StatusInfo.PlayerCount, string.Format("Players: {0}", _playerController.Count()));
            SetStatusInfo((int)StatusInfo.RoomCount, string.Format("Rooms: {0}", _roomController.Count()));
            SetStatusInfo((int)StatusInfo.RoomPlaying, string.Format("Rooms Playing: {0}", _roomController.ListAllRooms().Where(r => r.IsPlaying).Count()));
        }
    }
}