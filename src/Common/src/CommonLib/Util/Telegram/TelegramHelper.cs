#if _SERVER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace CommonLib.Util.Telegram
{
    public class TelegramHelper
    {
        /// <summary>
        /// Token of bot from Telegram.
        /// </summary>
        private string _token = "";

        /// <summary>
        /// ID of Group in Telegram;
        /// </summary>
        private static int _groupID = 0;

        /// <summary>
        /// Client.
        /// </summary>
        private static TelegramBotClient _client = null;

        /// <summary>
        /// Server name.
        /// </summary>
        private string _serverName = "";
        public string ServerName
        {
            get { return _serverName; }
        }

        /// <summary>
        /// Log Filter
        /// </summary>
        private CLogType _logType = CLogType.Debug;
        public CLogType LogType
        {
            get { return _logType; }
        }

        public void Setup(string serverName, string token, int groupID, CLogType logType)
        {
            _serverName = serverName;
            _token = token;
            _groupID = groupID;
            _logType = logType;

            _client = new TelegramBotClient(_token);

            _client.OnReceiveError += BotOnReceiveError;
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            CLog.E("Received error: {0} — {1}", e.ApiRequestException.ErrorCode, e.ApiRequestException.Message);
        }

        public void SendMessage(string message)
        {
            string newMessage = string.Format("[{0}] - {1}", _serverName, message);
            AsyncSendMessage(newMessage);
        }

        private static async void AsyncSendMessage(string message)
        {
            try
            {
                await _client.SendTextMessageAsync(_groupID, message);
            }
            catch { }
        }

        private static async void SendFileMessage(string filePath, string caption = "")
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await _client?.SendDocumentAsync(_groupID, fileStream, caption);
            }
        }
    }
}
#endif