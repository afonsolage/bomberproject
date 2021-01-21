#if _SERVER
using CommonLib.DB;
using CommonLib.Util;
using CommonLib.Util.Telegram;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CommonLib.Server
{
    public class BaseServer
    {
        protected readonly string _dbConfigFile;

        protected string _name;
        protected string _version;

        protected readonly uint _iid;

        private readonly Dictionary<string, string> _config;

        protected bool _running;
        protected bool _closeRequested;
        protected bool _logPaused;

        protected TelegramHelper _telegramHelper;

        protected Dictionary<int, string> _pendingStatusUpdate;
        protected ConcurrentBag<Tuple<ConsoleColor, string>> _pendingLogs;

        public BaseServer(uint instanceId, string name, string version)
        {
            _iid = instanceId;
            _name = name;
            _version = version;
            _config = new Dictionary<string, string>();
            _logPaused = false;

            _dbConfigFile = "db.cfg";

            _closeRequested = false;
            _pendingStatusUpdate = new Dictionary<int, string>();
            _pendingLogs = new ConcurrentBag<Tuple<ConsoleColor, string>>();
        }

        public virtual bool Init()
        {
            SetupLog();

            SetupDump();

            ConnectionFactory.LoadConfigFile(_dbConfigFile);

            LoadDBConfig();

            return true;
        }

        #region DUMP
        public virtual bool SetupDump()
        {
            /* NBug configuration. */

            // Attach exception handlers after all configuration is done
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            TaskScheduler.UnobservedTaskException += NBug.Handler.UnobservedTaskException;

            // Check if path exits, if not, let to create.
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Exceptions";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // Settings
            NBug.Settings.MiniDumpType = NBug.Enums.MiniDumpType.Full;
            NBug.Settings.StoragePath = path;
            NBug.Settings.UIMode = NBug.Enums.UIMode.None;
            NBug.Settings.ExitApplicationImmediately = false;
            NBug.Settings.SleepBeforeSend = 0;

            return true;
        }
        #endregion

        public virtual bool Start()
        {
            _running = true;
            return true;
        }

        protected void LoadDBConfig()
        {
            using (var con = new DBConnection("config"))
            {
                var reader = con.Query("SELECT name, value FROM configuration WHERE instance_id = @p1;", _iid);

                while (reader.Read())
                {
                    _config.Add(reader.GetString("name"), reader.GetString("value"));
                }
            }
        }

        public virtual void ShowConfigInfo()
        {
            CLog.D("Configuration loaded: ");
            foreach (var entry in _config)
            {
                CLog.I("\t{0}: {1}", entry.Key, entry.Value);
            }
        }

        public void SetupTelegram(string serverName, string token, int groupID, CLogType logType)
        {
            _telegramHelper = new TelegramHelper();
            _telegramHelper.Setup(serverName, token, groupID, logType);
        }

        public void Quit()
        {
            if (_closeRequested)
                return;

            CLog.F("Quitting application...");

            CLog.Close();

            _closeRequested = true;
            _running = false;
        }

        protected virtual void OnClose()
        {

        }

        private void HandleLogCommand(string[] command)
        {
            if (command.Length < 2)
            {
                CLog.W("Log command: log <level>");
                return;
            }

            var cmd = command[1];

            switch (cmd)
            {
                case "level":
                    {
                        ProcessLogLevel(command);
                    }
                    break;
                case "pause":
                    {
                        CLog.W("Log generation was paused!");
                        CLog.Pause();
                    }
                    break;
                case "unpause":
                    {
                        CLog.Unpause();
                        CLog.W("Log generation was resumed!");
                    }
                    break;
                default:
                    {
                        CLog.W("Log command: log <level>");
                        return;
                    }
            }
        }

        private void ProcessLogLevel(string[] command)
        {
            if (command.Length < 3)
            {
                CLog.W("Log command: log level <debug|info|success|warn|error|fatal>");
                return;
            }

            switch (command[2])
            {
                case "debug":
                    {
                        CLog.filter = CLogType.Debug;
                    }
                    break;
                case "info":
                    {
                        CLog.filter = CLogType.Info;
                    }
                    break;
                case "success":
                    {
                        CLog.filter = CLogType.Success;
                    }
                    break;
                case "warn":
                    {
                        CLog.filter = CLogType.Warn;
                    }
                    break;
                case "error":
                    {
                        CLog.filter = CLogType.Error;
                    }
                    break;
                case "fatal":
                    {
                        CLog.filter = CLogType.Fatal;
                    }
                    break;
                default:
                    {
                        CLog.W("Log command: log level <debug|info|warn|error|fatal|success>");
                        return;
                    }
            }

            var bak = CLog.filter;
            CLog.filter = CLogType.Warn;
            CLog.W("Log level was set to {0}", command[2]);
            CLog.filter = bak;
        }

        protected virtual void ProcessCommand(string[] command)
        {
        }

        protected virtual void SetupLog()
        {
            CLog.EnableLogOnFile = true;

            CLog.writter = (CLogType type, string formattedMessage) =>
            {
                ConsoleColor lineColor = ConsoleColor.Black;

                switch (type)
                {
                    case CLogType.Success:
                        lineColor = ConsoleColor.Green;
                        break;
                    case CLogType.Fatal:
                        lineColor = ConsoleColor.Magenta;
                        break;
                    case CLogType.Error:
                        lineColor = ConsoleColor.Red;
                        break;
                    case CLogType.Warn:
                        lineColor = ConsoleColor.Yellow;
                        break;
                    case CLogType.Info:
                        lineColor = ConsoleColor.Gray;
                        break;
                    case CLogType.Debug:
                        lineColor = ConsoleColor.Cyan;
                        break;
                }

                if (_logPaused)
                {
                    _pendingLogs.Add(new Tuple<ConsoleColor, string>(lineColor, formattedMessage));
                }
                else
                {
                    Console.ForegroundColor = lineColor;
                    Console.WriteLine(formattedMessage);
                }

                // Telegram.
                if (_telegramHelper != null)
                {
                    if (type <= _telegramHelper.LogType)
                        _telegramHelper.SendMessage(formattedMessage);
                }
            };
        }

        protected string GetConfig(string name, string defaultValue = "")
        {
            if (_config.ContainsKey(name))
                return _config[name];
            else
                return defaultValue;
        }

        protected void StartEventLoop()
        {
            while (_running)
            {
                Thread.Sleep(10);

                if (!_pendingLogs.IsEmpty)
                {
                    while (_pendingLogs.Count > 0)
                    {
                        if (_pendingLogs.TryTake(out var tuple))
                        {
                            Console.ForegroundColor = tuple.Item1;
                            Console.WriteLine(tuple.Item2);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Quit();
                    }
                    else if (key.Key == ConsoleKey.F1)
                    {
                        _logPaused = true;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.Write("Command: ");
                        var cmd = Console.ReadLine().Split(' ');
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                        _logPaused = false;

                        if (cmd.Length == 0)
                        {
                            continue;
                        }

                        if (cmd[0] == "log")
                        {
                            HandleLogCommand(cmd);
                        }
                        else
                        {
                            ProcessCommand(cmd);
                        }
                    }
                }
            }

            Environment.Exit(0);
        }
    }
}

#endif