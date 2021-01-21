using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.DB;
using CommonLib.Networking;
using CommonLib.Util;
using RoomServer.Server.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Server
{
    class DatabaseClient : ClientSocket
    {
        private AppServer _app;
        public AppServer App
        {
            get
            {
                return _app;
            }
        }

        public DatabaseClient(AppServer app, string serverHost, int serverPort) : base(serverHost, serverPort)
        {
            _app = app;
        }

        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            switch (rawMessage.MsgType)
            {
                case MessageType.DX_TOKEN_PLAYER_RES:
                    DatabaseMessages.TokenPlayerRes(rawMessage.To<DX_TOKEN_PLAYER_RES>(), this);
                    break;
                case MessageType.DR_LIST_CELL_TYPES_RES:
                    DatabaseMessages.ListCellTypeRes(rawMessage.To<DR_LIST_CELL_TYPES_RES>(), this);
                    break;
                case MessageType.DR_LIST_MAP_RES:
                    DatabaseMessages.ListMapRes(rawMessage.To<DR_LIST_MAP_RES>(), this);
                    break;
                case MessageType.DR_LIST_POWERUP_RES:
                    DatabaseMessages.ListPowerUpRes(rawMessage.To<DR_LIST_POWERUP_RES>(), this);
                    break;
                default:
                    CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                    break;
            }

        }
    }
}
