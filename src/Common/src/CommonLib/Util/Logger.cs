using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CommonLib.Util
{
    public enum CLogType
    {
        Fatal,
        Error,
        Warn,
        Success,
        Info,
        Debug,
    }

    public static class CLog
    {
        private const string LOG_FOLDER_NAME = "logs";

        public delegate void MessageWritter(CLogType type, string formattedMessage);
        public static MessageWritter writter;

        /// <summary>
        /// Sets a log filter. Only messages of this level or higher will be logger.
        /// </summary>
        public static CLogType filter = CLogType.Debug;
        private static bool _paused = false;

        private static string _logPath;
        /// <summary>
        /// If EnableLogOnFile is enable, this path indicates where the log folder will be created. If this path is empty, it'll default to application path.
        /// </summary>
        public static string LogPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logPath))
                {
                    _logPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                }
                return _logPath;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.EndsWith("/"))
                {
                    value = value.TrimEnd('/');
                }
                _logPath = value;
            }
        }

        private static string LogFolderPath { get => LogPath + "/" + LOG_FOLDER_NAME; }

        private static string _generalFileName;
        private static string LogGeneralPath
        {
            get
            {
                if (string.IsNullOrEmpty(_generalFileName))
                {
                    var currentDate = DateTime.Now;
                    _generalFileName = string.Format("General_{0:0000}{1:00}{2:00}-{3:00}{4:00}.log", currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute);
                }
                return LogFolderPath + "/" + _generalFileName;
            }
        }

        private static string _exceptionFileName;
        private static string LogExceptionPath
        {
            get
            {
                if (string.IsNullOrEmpty(_exceptionFileName))
                {
                    var currentDate = DateTime.Now;
                    _exceptionFileName = string.Format("Exception_{0:0000}{1:00}{2:00}-{3:00}{4:00}.log", currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute);
                }
                return LogFolderPath + "/" + _exceptionFileName;
            }
        }

        private static bool _logOnFile = false;
        /// <summary>
        /// Enables or disables the log writing on a disk file. By default it's value is false.
        /// </summary>
        public static bool EnableLogOnFile
        {
            get => _logOnFile;
            set
            {
                if (value)
                {
                    if (!Directory.Exists(LogFolderPath))
                    {
                        Directory.CreateDirectory(LogFolderPath);
                    }
                }
                _logOnFile = value;
            }
        }

        public static void Close()
        {
            lock (_logLock)
            {
                _logStreamWriter?.Close();
                _logStreamWriter = null;
            }

            lock (_exceptionLock)
            {
                _exceptionStreamWriter?.Close();
                _exceptionStreamWriter = null;
            }
        }

        /// <summary>
        /// Lock of StreamWriter.
        /// </summary>
        private static object _logLock = new Object();

        /// <summary>
        /// Stream Writer.
        /// </summary>
        private static StreamWriter _logStreamWriter;

        private static DateTime _generalLogDate = DateTime.Today;
        public static void WriteLogOnFile(CLogType type, string message, params object[] parameters)
        {
            try
            {
                lock (_logLock)
                {
                    //If the log isn't from today, let's close it and force the logger to create a fresh new one.
                    if (_generalLogDate != DateTime.Today)
                    {
                        Close();
                    }

                    if (_logStreamWriter == null)
                    {
                        _logStreamWriter = new StreamWriter(new FileStream(LogGeneralPath, FileMode.Append, FileAccess.Write, FileShare.Read));
                        _generalLogDate = DateTime.Today;
                    }

                    var currentDate = DateTime.Now;
                    var formattedMsg = string.Format(string.Format("{0:00}:{1:00}:{2:00}\t", currentDate.Hour, currentDate.Minute, currentDate.Second) +
                        "[" + type.ToString() + "] - \t" +
                        message,
                        parameters);

                    _logStreamWriter.WriteLine(formattedMsg);
                    _logStreamWriter.Flush();
                }
            }
            catch (Exception e)
            {
                _logOnFile = false;
                F(string.Format("Failed to write log on file. Error: {0}", e.Message));
                _logOnFile = true;
            }
        }

        /// <summary>
        /// Lock of StreamWriter.
        /// </summary>
        private static object _exceptionLock = new Object();

        /// <summary>
        /// Stream Writer.
        /// </summary>
        private static StreamWriter _exceptionStreamWriter;

        private static DateTime _exceptionLogDate = DateTime.Today;
        public static void WriteExceptionOnFile(Exception e)
        {
            try
            {
                lock (_exceptionLock)
                {
                    //If the log isn't from today, let's close it and force the logger to create a fresh new one.
                    if (_exceptionLogDate != DateTime.Today)
                    {
                        Close();
                    }

                    if (_exceptionStreamWriter == null)
                    {
                        _exceptionStreamWriter = new StreamWriter(new FileStream(LogExceptionPath, FileMode.Append, FileAccess.Write, FileShare.Read));
                        _exceptionLogDate = DateTime.Today;
                    }

                    var currentDate = DateTime.Now;
                    var formattedMsg = string.Format("{0:00}:{1:00}:{2:00}\t", currentDate.Hour, currentDate.Minute, currentDate.Second) + e.ToString();

                    _exceptionStreamWriter.WriteLine(formattedMsg);
                    _exceptionStreamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                _logOnFile = false;
                F(string.Format("Failed to write exception on file. Error: {0}", ex.Message));
                _logOnFile = true;
            }
        }

        public static void Pause()
        {
            _paused = true;
        }

        public static void Unpause()
        {
            _paused = false;
        }

        public static void S(string message, params object[] parameters)
        {
            Write(CLogType.Success, message, parameters);
        }

        public static void E(string message, params object[] parameters)
        {
            Write(CLogType.Error, message, parameters);
        }

        public static void W(string message, params object[] parameters)
        {
            Write(CLogType.Warn, message, parameters);
        }

        public static void D(string message, params object[] parameters)
        {
            Write(CLogType.Debug, message, parameters);
        }

        public static void F(string message, params object[] parameters)
        {
            Write(CLogType.Fatal, message, parameters);
        }

        public static void I(string message, params object[] parameters)
        {
            Write(CLogType.Info, message, parameters);
        }

        public static void Catch(Exception e)
        {
            Write(CLogType.Error, "Exception {0} caught. Error: {1}", e.GetType().Name, e.Message);

            if (EnableLogOnFile)
            {
                WriteExceptionOnFile(e);
            }
        }

        private static void Write(CLogType type, string message, params object[] parameters)
        {
            if ((int)type > (int)filter)
                return;

            if (!_paused && writter != null)
            {
                var formattedMsg = string.Format(message, parameters);

                formattedMsg = formattedMsg.Replace("\r\n", "\t"); //Removes all line breaks. since this would break the console

                writter(type, formattedMsg);
            }

            if (EnableLogOnFile)
                WriteLogOnFile(type, message, parameters);
        }
    }
}
