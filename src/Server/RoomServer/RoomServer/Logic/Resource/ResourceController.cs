using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Resource
{
    public class ResourceController
    {
        private string _pathResource;
        public string PathResource
        {
            get { return _pathResource; }
            set { _pathResource = value; }
        }

        private int _logFilter;
        public int LogFilter
        {
            get { return _logFilter; }
            set { _logFilter = value; }
        }

        #region Telegram
        private bool _telegramEnabled;
        public bool TelegramEnabled
        {
            get { return _telegramEnabled; }
            set { _telegramEnabled = value; }
        }

        private string _telegramToken;
        public string TelegramToken
        {
            get { return _telegramToken; }
            set { _telegramToken = value; }
        }

        private int _telegramGroupID;
        public int TelegramGroupID
        {
            get { return _telegramGroupID; }
            set { _telegramGroupID = value; }
        }

        private int _telegramLogFilter;
        public int TelegramLogFilter
        {
            get { return _telegramLogFilter; }
            set { _telegramLogFilter = value; }
        }
        #endregion

        public void Initialization()
        {
            
        }
    }
}
