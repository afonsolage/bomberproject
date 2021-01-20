using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.DB;
using CommonLib.Networking;
using CommonLib.Util;
using DBServer.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBServer.Server
{
    class ClientSession : ClientConnection
    {
        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            switch (rawMessage.MsgType)
            {
                //COOMON SERVER MESSAGES
                case MessageType.DX_TOKEN_PLAYER_REQ:
                    LobbyServer.TokenPlayerReq(rawMessage.To<DX_TOKEN_PLAYER_REQ>(), this);
                    break;

                //ROOM SERVER MESSAGES
                case MessageType.DR_STARTUP_INFO_REQ:
                    RoomServer.StartupInfoReq(rawMessage.To<DR_STARTUP_INFO_REQ>(), this);
                    break;

                //LOBBY SERVER MESSAGES
                case MessageType.DL_STARTUP_INFO_REQ:
                    LobbyServer.StartupInfoReq(rawMessage.To<DL_STARTUP_INFO_REQ>(), this);
                    break;
                case MessageType.DL_REGISTER_REQ:
                    LobbyServer.RegisterReq(rawMessage.To<DL_REGISTER_REQ>(), this);
                    break;
                case MessageType.DL_AUTH_PLAYER_REQ:
                    LobbyServer.AuthPlayerReq(rawMessage.To<DL_AUTH_PLAYER_REQ>(), this);
                    break;
                case MessageType.DL_FB_AUTH_REQ:
                    LobbyServer.FbAuthReq(rawMessage.To<DL_FB_AUTH_REQ>(), this);
                    break;
                case MessageType.DL_PLAYER_ADD_INFO_REQ:
                    LobbyServer.PlayerAddInfoReq(rawMessage.To<DL_PLAYER_ADD_INFO_REQ>(), this);
                    break;
                case MessageType.DL_PLAYER_CREATE_REQ:
                    LobbyServer.PlayerCreateReq(rawMessage.To<DL_PLAYER_CREATE_REQ>(), this);
                    break;
                case MessageType.DL_FRIEND_REQUEST_REQ:
                    LobbyServer.FriendRequestReq(rawMessage.To<DL_FRIEND_REQUEST_REQ>(), this);
                    break;
                case MessageType.DL_FRIEND_RESPONSE_REQ:
                    LobbyServer.FriendResponseReq(rawMessage.To<DL_FRIEND_RESPONSE_REQ>(), this);
                    break;
                case MessageType.DL_FRIEND_REMOVE_REQ:
                    LobbyServer.FriendRemoveReq(rawMessage.To<DL_FRIEND_REMOVE_REQ>(), this);
                    break;
                default:
                    CLog.W("Unrecognized message type: {0}.", rawMessage.MsgType);
                    break;
            }

        }

        internal void Setup()
        {
            Ready();
        }
    }
}
