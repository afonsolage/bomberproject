#if _SERVER
using CommonLib.DB;
using CommonLib.Util;
using CommonLib.Util.Telegram;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLib.Server
{
    public class BaseServer
    {
        protected readonly string _dbConfigFile;

        protected string _name;
        protected string _version;

        protected readonly uint _iid;

        private readonly Dictionary<string, string> _config;

        protected Form _mainForm;
        //protected ScrollBar _scrollBar;
        protected LogComponent _logPanel;
        protected TextBox _commandInput;
        protected StatusBar _statusBar;

        protected bool _running;
        protected bool _closeRequested;

        protected TelegramHelper _telegramHelper;

        protected Dictionary<int, string> _pendingStatusUpdate;

        public BaseServer(uint instanceId, string name, string version)
        {
            _iid = instanceId;
            _name = name;
            _version = version;
            _config = new Dictionary<string, string>();

            _dbConfigFile = "db.cfg";

            _closeRequested = false;
            _pendingStatusUpdate = new Dictionary<int, string>();
        }

        public virtual bool Init()
        {
            CreateForm();
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
            Application.ThreadException += NBug.Handler.ThreadException;
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
            _mainForm.Show();
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

        protected virtual void CreateForm()
        {
            _mainForm = new Form
            {
                Text = _name + " " + _version,
                MinimumSize = new Size(800, 600)
            };
            _mainForm.FormClosed += OnFormClosed;

            SetupFormComponents();
        }

        protected virtual void SetupFormComponents()
        {
            _logPanel = new LogComponent
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Font = new Font("Lucida Console", 12),
                AutoScroll = true,
            };

            _commandInput = new TextBox
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                Font = new Font("Lucida Console", 12),

                Multiline = true,
                WordWrap = true
            };
            _commandInput.KeyDown += OnInputKeyDown;

            //_mainForm.Controls.Add(_scrollBar);
            _mainForm.Controls.Add(_logPanel);
            _mainForm.Controls.Add(_commandInput);

            _statusBar = new StatusBar()
            {
                Visible = false,
                ShowPanels = true,
            };

            _mainForm.Controls.Add(_statusBar);
        }

        protected int AddStatusInfo()
        {
            _statusBar.Visible = true;

            return _statusBar.Panels.Add(new StatusBarPanel()
            {
                BorderStyle = StatusBarPanelBorderStyle.None,
                Text = "Fill the rest!",
                AutoSize = StatusBarPanelAutoSize.Spring,
            });
        }

        protected void SetStatusInfo(int index, string text)
        {
            lock (_pendingStatusUpdate)
            {
                _pendingStatusUpdate[index] = text;
            }
        }

        public void SetupTelegram(string serverName, string token, int groupID, CLogType logType)
        {
            _telegramHelper = new TelegramHelper();
            _telegramHelper.Setup(serverName, token, groupID, logType);
        }

        public void Quit()
        {
            CLog.F("Quitting application...");

            CLog.Close();

            _closeRequested = true;
        }

        protected virtual void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            _running = false;
            CLog.W("Shutting down application due to form close request...");
            OnClose();
        }

        protected virtual void OnClose()
        {
            
        }

        private List<string> _commandHistory = new List<string>();
        private int _commandHistoryIdx;
        private void AddCommandBuffer()
        {
            _commandHistory.Add(_commandInput.Text);
            _commandHistoryIdx = _commandHistory.Count;
        }

        private void CommandBufferDown()
        {
            if (_commandHistoryIdx >= _commandHistory.Count - 1)
            {
                _commandInput.Text = "";
                return;
            }
            _commandHistoryIdx++;
            _commandInput.Text = _commandHistory[_commandHistoryIdx];

            _commandInput.Focus();
            _commandInput.SelectionStart = _commandInput.Text.Length + 1;
        }

        private void CommandBufferUp()
        {
            if (_commandHistoryIdx <= 0)
            {
                return;
            }
            _commandHistoryIdx--;
            _commandInput.Text = _commandHistory[_commandHistoryIdx];

            _commandInput.Focus();
            _commandInput.SelectionStart = _commandInput.Text.Length + 1;
        }

        protected virtual void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                CommandBufferUp();
            }
            else if (e.KeyCode == Keys.Down)
            {
                CommandBufferDown();
            }
            else if (e.KeyCode == Keys.Enter && _commandInput.Text.Length > 1)
            {
                //Remove the \r\n characters
                _commandInput.Text = _commandInput.Text.Replace("\r\n", "");

                var parameters = _commandInput.Text.Split(' ');

                if (parameters?.Length > 0)
                {
                    var name = parameters[0];

                    if (name == "clear")
                    {
                        ClearConsole();
                    }
                    else if (name == "log")
                    {
                        HandleLogCommand(parameters);
                    }
                    else
                    {
                        AddCommandBuffer();
                        ProcessCommand(parameters);
                    }
                }

                _commandInput.Text = "";
            }
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
                Color lineColor = Color.Black;

                switch (type)
                {
                    case CLogType.Success:
                        lineColor = Color.Green;
                        break;
                    case CLogType.Fatal:
                        lineColor = Color.Purple;
                        break;
                    case CLogType.Error:
                        lineColor = Color.Red;
                        break;
                    case CLogType.Warn:
                        lineColor = Color.Yellow;
                        break;
                    case CLogType.Info:
                        lineColor = Color.LightGray;
                        break;
                    case CLogType.Debug:
                        lineColor = Color.Cyan;
                        break;
                }

                _logPanel.AddLogText(formattedMessage, lineColor);

                // Telegram.
                if(_telegramHelper != null)
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
                if (_closeRequested)
                    _mainForm.Close();

                UpdatePendingStatus();

                Application.DoEvents();

                Thread.Sleep(10);
            }
        }

        private void UpdatePendingStatus()
        {
            if (_pendingStatusUpdate.Count > 0)
            {
                lock (_pendingStatusUpdate)
                {
                    foreach (var pair in _pendingStatusUpdate)
                    {
                        _statusBar.Panels[pair.Key].Text = pair.Value;
                    }

                    _pendingStatusUpdate.Clear();
                }
            }
        }

        protected void ClearConsole()
        {
            _logPanel.Clear();
        }
    }
}

#endif