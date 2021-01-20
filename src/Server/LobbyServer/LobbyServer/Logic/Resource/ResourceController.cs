using CommonLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LobbyServer.Logic.Resource
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

        private ulong[] _expTable;
        public uint MaxLevel { get; set; }

        public void Initialization()
        {
            LoadLevelUp();
        }

        private void LoadLevelUp()
        {
            var xml = XElement.Load(PathResource + "/LevelUp.xml");
            var elements = xml.Elements("Experience");
            if(elements != null)
            {
                int count = elements.Count();
                MaxLevel = (uint)count - 1;

                _expTable = new ulong[count];
                for(int i = 0; i < count; i++)
                {
                    _expTable[i] = Convert.ToUInt64(elements.Single(X => X.Attribute("Level").Value == i.ToString()).Attribute("Value").Value);
                }
            }
        }

        public bool GetNextExpTable(uint level)
        {
            if (level > MaxLevel)
            {
                return false;
            }

            return (level > MaxLevel) ? false : true;
        }

        public bool GetExpTable(uint level, out ulong experience)
        {
            if(level > MaxLevel)
            {
                experience = 0;

                CLog.E("This should not happen.");
                return false;
            }

            experience = _expTable[level];

            return true;
        }
    }
}
