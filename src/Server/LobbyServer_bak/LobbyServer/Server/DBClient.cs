using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.DB;
using CommonLib.Networking;
using CommonLib.Util;
using LobbyServer.Server.Messaging;

namespace LobbyServer.Server
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
                case MessageType.DL_AUTH_PLAYER_RES:
                    DatabaseMessages.AuthPlayerRes(rawMessage.To<DL_AUTH_PLAYER_RES>(), this);
                    break;
                case MessageType.DL_FB_AUTH_RES:
                    DatabaseMessages.FbAuthRes(rawMessage.To<DL_FB_AUTH_RES>(), this);
                    break;
                case MessageType.DL_REGISTER_RES:
                    DatabaseMessages.RegisterRes(rawMessage.To<DL_REGISTER_RES>(), this);
                    break;
                case MessageType.DL_LIST_MAP_RES:
                    DatabaseMessages.ListMapRes(rawMessage.To<DL_LIST_MAP_RES>(), this);
                    break;
                case MessageType.DL_PLAYER_CREATE_RES:
                    DatabaseMessages.PlayerCreateRes(rawMessage.To<DL_PLAYER_CREATE_RES>(), this);
                    break;
                case MessageType.DL_PLAYER_ADD_INFO_RES:
                    DatabaseMessages.PlayerAddInfoRes(rawMessage.To<DL_PLAYER_ADD_INFO_RES>(), this);
                    break;
                case MessageType.DL_FRIEND_REQUEST_RES:
                    DatabaseMessages.FriendRequestRes(rawMessage.To<DL_FRIEND_REQUEST_RES>(), this);
                    break;
                case MessageType.DL_FRIEND_RESPONSE_RES:
                    DatabaseMessages.FriendResposeRes(rawMessage.To<DL_FRIEND_RESPONSE_RES>(), this);
                    break;
                case MessageType.DL_FRIEND_REMOVE_RES:
                    DatabaseMessages.FriendRemoveRes(rawMessage.To<DL_FRIEND_REMOVE_RES>(), this);
                    break;
                default:
                    CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                    break;
            }

        }
    }
}
