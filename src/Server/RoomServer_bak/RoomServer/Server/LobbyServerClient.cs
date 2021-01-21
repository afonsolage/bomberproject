using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.DB;
using CommonLib.Messaging.Lobby;
using CommonLib.Networking;
using CommonLib.Util;
using RoomServer.Server.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Server
{
    class LobbyServerClient : ClientSocket
    {
        private AppServer _app;
        public AppServer App
        {
            get
            {
                return _app;
            }
        }

        public LobbyServerClient(AppServer app, string serverHost, int serverPort) : base(serverHost, serverPort)
        {
            _app = app;
        }

        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            switch (rawMessage.MsgType)
            {
                case MessageType.LR_WELCOME_NFY:
                    LobbyServerMessages.WelcomeNfy(rawMessage.To<LR_WELCOME_NFY>(), this);
                    break;
                case MessageType.LR_CREATE_ROOM_REQ:
                    LobbyServerMessages.CreateRoomReq(rawMessage.To<LR_CREATE_ROOM_REQ>(), this);
                    break;
                case MessageType.LR_DUMMY_JOIN_NFY:
                    LobbyServerMessages.DummyJoinNfy(rawMessage.To<LR_DUMMY_JOIN_NFY>(), this);
                    break;
                default:
                    CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                    break;
            }
        }

        protected override bool OnDisconnect()
        {
            base.OnDisconnect();

            _app.RoomManager.ForceFinishAllRooms();

            return true; //Tries to reconnect
        }
    }
}
