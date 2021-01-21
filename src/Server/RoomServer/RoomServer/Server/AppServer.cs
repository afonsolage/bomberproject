using CommonLib.DB;
using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Networking;
using CommonLib.Server;
using CommonLib.Util;
using RoomServer.Logic;
using RoomServer.Logic.Object;
using RoomServer.Logic.PowerUP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Messaging.Lobby;
using RoomServer.Logic.Resource;
using System.Xml.Linq;

namespace RoomServer.Server
{
    internal partial class AppServer : GameLoopServer
    {
        private static readonly uint TICKS_PER_SECOND = 60;

        enum StatusInfo
        {
            PlayerCount,
            RoomCount,

            MAX, //Don't change this!
        }

        private Thread _socketThread;
        private ServerSocket<ClientSession> _socketServer;

        private DatabaseClient _dbClient;
        public DatabaseClient DBClient { get => _dbClient; }

        private LobbyServerClient _lobbyClient;
        public LobbyServerClient LobbyClient { get => _lobbyClient; }

        private RoomManager _roomManager;
        public RoomManager RoomManager { get => _roomManager; }

        private readonly ResourceController _resourceController;
        public ResourceController ResourceController => _resourceController;

        public int Capacity { get; private set; }
        public string PublicIP { get; private set; }
        public int PublicPort { get; private set; }

        private readonly List<WeakReference<ClientSession>> _connectedSessions;
        private Timer _clearSessionsTimer;
        private Timer _checkClosedSessionsTimer;

        private object _readyLock;

        private bool _roomManagerRdy;
        public bool RoomManagerReady
        {
            set
            {
                lock (_readyLock)
                {
                    CLog.I("RoomManager is ready.");
                    _roomManagerRdy = true;
                    CheckReady();
                }
            }
        }

        public AppServer(uint instanceId) : base(instanceId, Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), TICKS_PER_SECOND)
        {
            _readyLock = new object();
            _roomManagerRdy = false;

            _connectedSessions = new List<WeakReference<ClientSession>>();

            _roomManager = new RoomManager(this);
            _resourceController = new ResourceController();
        }

        public override bool Init()
        {
            if (!base.Init())
                return false;

            //for (var i = 0; i < (int)StatusInfo.MAX; i++)
            //{
            //    AddStatusInfo();
            //}

            CLog.D("    _____   ____   ____  __  __  _____ ______ _______      ________ _____  ");
            CLog.D("   |  __ \\ / __ \\ / __ \\|  \\/  |/ ____|  ____|  __ \\ \\    / /  ____|  __ \\ ");
            CLog.D("   | |__) | |  | | |  | | \\  / | (___ | |__  | |__) \\ \\  / /| |__  | |__) |");
            CLog.D("   |  _  /| |  | | |  | | |\\/| |\\___ \\|  __| |  _  / \\ \\/ / |  __| |  _  / ");
            CLog.D("   | | \\ \\| |__| | |__| | |  | |____) | |____| | \\ \\  \\  /  | |____| | \\ \\ ");
            CLog.D("   |_|  \\_\\\\____/ \\____/|_|  |_|_____/|______|_|  \\_\\  \\/   |______|_|  \\_\\");
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
                SetupTelegram("RoomServer", ResourceController.TelegramToken, ResourceController.TelegramGroupID, (CLogType)ResourceController.TelegramLogFilter);
                _telegramHelper.SendMessage(welcomeMsg);
            }

            _roomManager.Init();

            CLog.filter = (CLogType)_resourceController.LogFilter;

            return true;
        }

        private void SetupNetworking()
        {
            Capacity = int.Parse(GetConfig("capacity", "2000"));
            PublicIP = GetConfig("publicAddress", "127.0.0.1");
            PublicPort = int.Parse(GetConfig("listenPort", "9876"));

            string dbServerIP = GetConfig("dbServerAddress", "127.0.0.1");
            int dbServerPort = int.Parse(GetConfig("dbServerPort", "11510"));

            _dbClient = new DatabaseClient(this, dbServerIP, dbServerPort);
            _dbClient.Start();

            string lbServerIP = GetConfig("lbServerAddress", "127.0.0.1");
            int lbServerPort = int.Parse(GetConfig("lbServerPort", "11510"));

            _lobbyClient = new LobbyServerClient(this, lbServerIP, lbServerPort);
            _lobbyClient.Start();
        }

        private void InitResources()
        {
            XElement xml = null;

            try
            {
                xml = XElement.Load(@"RoomServer.xml");
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Failed to read {0}.", "RoomServer.xml"));
            }

            // Path of resource folder.
            _resourceController.PathResource = XMLHelper.GetSafeAttributeStr(xml, "Resource", "Path");

            // Log Filter.
            _resourceController.LogFilter = (int)XMLHelper.GetSafeAttribute(xml, "Log", "Filter");

            // Telegram information.
            _resourceController.TelegramEnabled = (bool)XMLHelper.GetSafeAttribute(xml, "Telegram", "Enabled");
            if (_resourceController.TelegramEnabled)
            {
                _resourceController.TelegramToken = XMLHelper.GetSafeAttributeStr(xml, "Telegram", "Token");
                _resourceController.TelegramGroupID = (int)XMLHelper.GetSafeAttribute(xml, "Telegram", "GroupID");
                _resourceController.TelegramLogFilter = (int)XMLHelper.GetSafeAttribute(xml, "Telegram", "LogFilter");
            }

            _resourceController.Initialization();
        }

        protected void CheckReady()
        {
            if (_roomManagerRdy)
            {
                Ready();
            }
        }

        protected void Ready()
        {
            CLog.I("Server capacity: {0} players.", Capacity);
            CLog.I("Public IP: {0}", PublicIP);
            CLog.I("Public Port: {0}", PublicPort);
            CLog.I("Everything is ready. Publishing server....");

            PublishServer();
        }

        public string GetGlobalConfig(string name, string defaultValue)
        {
            return GetConfig(name, defaultValue);
        }

        protected override void OnStart()
        {
            var oneMin = TimeSpan.FromMinutes(1).Milliseconds;
            var oneSec = TimeSpan.FromSeconds(1).Milliseconds;

            _clearSessionsTimer = new Timer(s => RemovedEmptySessionRef(), null, oneMin, oneMin);
            _checkClosedSessionsTimer = new Timer(s => CheckClosedSessions(), null, oneSec, oneSec);
        }

        protected override void OnClose()
        {
            base.OnClose();

            _clearSessionsTimer?.Dispose();
            _checkClosedSessionsTimer?.Dispose();
            _socketServer?.Stop();
            _lobbyClient?.Stop();
            _dbClient?.Stop();
        }

        private void PublishServer()
        {
            string listenAddr = GetConfig("listenAddress", "0.0.0.0");

            _socketServer = new ServerSocket<ClientSession>(listenAddr, PublicPort);
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

        private void RemovedEmptySessionRef()
        {
            lock (_connectedSessions)
            {
                _connectedSessions.RemoveAll((wref) => (wref.TryGetTarget(out var s)) ? !s.IsActive : true);
            }
        }

        private void CheckClosedSessions()
        {
            lock (_connectedSessions)
            {
                _connectedSessions.ForEach(w =>
                {
                    if (w.TryGetTarget(out var s) && !s.Connected)
                        s.Stop();
                });
            }
        }

        internal ClientSession FindSession(ulong index, string token)
        {
            WeakReference<ClientSession> wref;

            lock (_connectedSessions)
            {
                wref = _connectedSessions.Find((wr) => (wr.TryGetTarget(out var s)) ? s.DBID == index && s.Token == token && s.IsActive : false);
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

            var playerCount = _roomManager.PlayerCount();

            //SetStatusInfo((int)StatusInfo.PlayerCount, string.Format("Players: {0}", playerCount));
            //SetStatusInfo((int)StatusInfo.RoomCount, string.Format("Rooms: {0}", _roomManager.Count()));

            LobbyClient.Send(new LR_USER_COUNT_NFY()
            {
                count = (uint) playerCount,
            });
        }
    }
}