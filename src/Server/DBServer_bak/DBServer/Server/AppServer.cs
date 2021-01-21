using CommonLib.Networking;
using CommonLib.Server;
using CommonLib.Util;
using DBServer.Logic.Resource;
using System;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace DBServer.Server
{
    class AppServer : GameLoopServer
    {
        private static readonly uint TICKS_PER_SECOND = 60;

        private ServerSocket<ClientSession> _socketServer;
        private Thread _socketThread;

        private readonly ResourceController _resourceController;
        public ResourceController ResourceController => _resourceController;

        public AppServer(uint instanceId) : base(instanceId, Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version.ToString(), TICKS_PER_SECOND)
        {
            _resourceController = new ResourceController();
        }

        public override bool Init()
        {
            if (!base.Init())
                return false;

            CLog.D("             _____  ____   _____ ______ _______      ________ _____  ");
            CLog.D("            |  __ \\|  _ \\ / ____|  ____|  __ \\ \\    / /  ____|  __ \\ ");
            CLog.D("            | |  | | |_) | (___ | |__  | |__) \\ \\  / /| |__  | |__) |");
            CLog.D("            | |  | |  _ < \\___ \\|  __| |  _  / \\ \\/ / |  __| |  _  / ");
            CLog.D("            | |__| | |_) |____) | |____| | \\ \\  \\  /  | |____| | \\ \\ ");
            CLog.D("            |_____/|____/|_____/|______|_|  \\_\\  \\/   |______|_|  \\_\\");
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
                SetupTelegram("DBServer", ResourceController.TelegramToken, ResourceController.TelegramGroupID, (CLogType)ResourceController.TelegramLogFilter);
                _telegramHelper.SendMessage(welcomeMsg);
            }

            CLog.filter = (CLogType)_resourceController.LogFilter;

            return true;
        }

        protected override void OnStart()
        {
            _socketThread = new Thread(_socketServer.Start);
            _socketThread.Start();
        }

        protected override void OnClose()
        {
            base.OnClose();

            _socketServer.Stop();

            Environment.Exit(0);
        }

        private void SetupNetworking()
        {
            string listenAddr = GetConfig("listenAddress", "0.0.0.0");
            int port = int.Parse(GetConfig("listenPort", "9876"));

            _socketServer = new ServerSocket<ClientSession>(listenAddr, port);
            _socketServer.OnClientConnected += OnClientConnected;
        }

        private void InitResources()
        {
            XElement xml = null;

            try
            {
                xml = XElement.Load(@"DBServer.xml");
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Failed to read {0}.", "DBServer.xml"));
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

        private void OnClientConnected(ClientSession client)
        {
            client.Setup();
        }

        public override void Tick(float delta)
        {
            
        }
    }
}
