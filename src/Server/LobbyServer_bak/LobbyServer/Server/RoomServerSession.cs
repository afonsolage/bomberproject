using System;
using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Networking;
using CommonLib.Util;
using LobbyServer.Server.Messaging;
using CommonLib.Messaging.Lobby;
using LobbyServer.Logic.Server;
using System.Diagnostics;

namespace LobbyServer.Server
{
    class RoomServerSession : ClientConnection
    {
        protected RoomServerController _controller;
        public AppServer App
        {
            get
            {
                return _controller.App;
            }
        }

        // we should keep a weak reference to avoid circular dependency, since RoomServer already has a reference to RoomServerSession.
        private WeakReference<RoomServer> _server = new WeakReference<RoomServer>(null);
        public RoomServer Server
        {
            get
            {
                if (!_server.TryGetTarget(out var res) && _controller != null)
                {
                    res = _controller.FindServer(this);
                    _server = new WeakReference<RoomServer>(res);
                }
                return res;
            }
        }

        public void Setup(RoomServerController controller)
        {
            _controller = controller;
            Ready();
            SendWelcomeMessage();
        }

        protected void SendWelcomeMessage()
        {
            var server = _controller.FindServer(this);
#if _DEBUG
            Debug.Assert(server != null);
#endif
            if (server == null)
                Close();

            Send(new LR_WELCOME_NFY()
            {
                uid = server.UID,
            });
        }

        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            CLog.D("Received packet {0} from {1}", rawMessage.MsgType, _socket.RemoteEndPoint);

            switch (rawMessage.MsgType)
            {
                case MessageType.LR_CREATE_ROOM_RES:
                    RoomServerMessages.CreateRoomRes(rawMessage.To<LR_CREATE_ROOM_RES>(), this);
                    break;
                case MessageType.LR_ROOM_FINISHED_NFY:
                    RoomServerMessages.RoomFinishedNfy(rawMessage.To<LR_ROOM_FINISHED_NFY>(), this);
                    break;
                case MessageType.LR_USER_COUNT_NFY:
                    RoomServerMessages.UserCountNfy(rawMessage.To<LR_USER_COUNT_NFY>(), this);
                    break;
                case MessageType.LR_SERVER_INFO_NFY:
                    RoomServerMessages.ServerInfoNfy(rawMessage.To<LR_SERVER_INFO_NFY>(), this);
                    break;
                default:
                    CLog.W("Unknown message received: {0}", rawMessage.MsgType);
                    break;
            }
        }

        protected override void Close()
        {
            base.Close();
            _controller.OnDisconnect(this);
        }
    }
}
