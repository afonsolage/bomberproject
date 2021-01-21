using System;
using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Networking;
using CommonLib.Util;
using LobbyServer.Server.Messaging;
using LobbyServer.Logic;
using System.Linq;
using CommonLib.Messaging.Common;

namespace LobbyServer.Server
{
    class ClientSession : ClientConnection
    {
        private Player _player;
        public Player Player { get => _player; set => _player = value; }

        protected string _login;
        public virtual string Login { get => _login; set => _login = value; }

        public string DeviceID { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// Indicates whether the disconnection was on remote (client) side or note
        /// </summary>
        public virtual bool RemoteDisconnection { get => !_closeRequested; }

        protected AppServer _app;
        public AppServer App
        {
            get
            {
                return _app;
            }
        }

        public void Setup(AppServer app)
        {
            _app = app;

            Ready();

            SendWelcomeMessage();
        }

        protected void SendWelcomeMessage()
        {

        }

        protected bool CanProccessUnauthenticatedMessage(MessageType type)
        {
            return type == MessageType.CL_AUTH_REQ || type == MessageType.CL_FB_AUTH_REQ || type == MessageType.CX_TOKEN_REQ || type == MessageType.CL_REGISTER_REQ;
        }

        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            CLog.D("Received packet {0} from {1}", rawMessage.MsgType, _socket.RemoteEndPoint);

            if (_player == null && !CanProccessUnauthenticatedMessage(rawMessage.MsgType))
            {
                CLog.E("Received message on an unauthenticated session. ({0} - {1})", rawMessage.MsgType, _socket.RemoteEndPoint);
                Close();
                return;
            }

            switch (rawMessage.MsgType)
            {
                case MessageType.CL_AUTH_REQ:
                    ClientMessages.Auth(rawMessage.To<CL_AUTH_REQ>(), this);
                    break;
                case MessageType.CX_TOKEN_REQ:
                    ClientMessages.Token(rawMessage.To<CX_TOKEN_REQ>(), this);
                    break;
                case MessageType.CL_LOGOUT_REQ:
                    ClientMessages.LogOut(rawMessage.To<CL_LOGOUT_REQ>(), this);
                    break;
                case MessageType.CL_FB_AUTH_REQ:
                    ClientMessages.FbAuthReq(rawMessage.To<CL_FB_AUTH_REQ>(), this);
                    break;
                case MessageType.CL_REGISTER_REQ:
                    ClientMessages.RegisterReq(rawMessage.To<CL_REGISTER_REQ>(), this);
                    break;
                case MessageType.CL_PLAYER_HEARTBEAT_RES:
                    ClientMessages.PlayerHeartBeatRes(rawMessage.To<CL_PLAYER_HEARTBEAT_RES>(), this);
                    break;
                case MessageType.CL_PLAYER_CREATE_REQ:
                    ClientMessages.PlayerCreateReq(rawMessage.To<CL_PLAYER_CREATE_REQ>(), this);
                    break;
                case MessageType.CL_CHAT_NORMAL_REQ:
                    ClientMessages.ChatNormalReq(rawMessage.To<CL_CHAT_NORMAL_REQ>(), this);
                    break;
                case MessageType.CL_CHAT_WHISPER_REQ:
                    ClientMessages.ChatWhisperReq(rawMessage.To<CL_CHAT_WHISPER_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_CREATE_REQ:
                    ClientMessages.RoomCreateReq(rawMessage.To<CL_ROOM_CREATE_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_JOIN_REQ:
                    ClientMessages.RoomJoinReq(rawMessage.To<CL_ROOM_JOIN_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_LEAVE_REQ:
                    ClientMessages.RoomLeaveReq(rawMessage.To<CL_ROOM_LEAVE_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_START_REQ:
                    ClientMessages.RoomStartReq(rawMessage.To<CL_ROOM_START_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_KICK_PLAYER_REQ:
                    ClientMessages.RoomKickPlayerReq(rawMessage.To<CL_ROOM_KICK_PLAYER_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_TRANSFER_OWNER_REQ:
                    ClientMessages.RoomTransferOwnerReq(rawMessage.To<CL_ROOM_TRANSFER_OWNER_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_CHANGE_SLOT_POS_REQ:
                    ClientMessages.RoomChangeSlotPosReq(rawMessage.To<CL_ROOM_CHANGE_SLOT_POS_REQ>(), this);
                    break;
                case MessageType.CL_PLAYER_READY_REQ:
                    ClientMessages.PlayerReadyReq(rawMessage.To<CL_PLAYER_READY_REQ>(), this);
                    break;
                case MessageType.CL_PLAYER_LOBBY_LIST_REQ:
                    ClientMessages.LobbyListReq(rawMessage.To<CL_PLAYER_LOBBY_LIST_REQ>(), this);
                    break;
                case MessageType.CL_ROOM_SETTING_REQ:
                    ClientMessages.RoomSettingReq(rawMessage.To<CL_ROOM_SETTING_REQ>(), this);
                    break;
                case MessageType.CL_FRIEND_REQUEST_REQ:
                    ClientMessages.FriendRequestReq(rawMessage.To<CL_FRIEND_REQUEST_REQ>(), this);
                    break;
                case MessageType.CL_FRIEND_RESPONSE_REQ:
                    ClientMessages.FriendResponseReq(rawMessage.To<CL_FRIEND_RESPONSE_REQ>(), this);
                    break;
                case MessageType.CL_FRIEND_REMOVE_REQ:
                    ClientMessages.FriendRemoveReq(rawMessage.To<CL_FRIEND_REMOVE_REQ>(), this);
                    break;
                default:
                    CLog.W("Unknown message received: {0}", rawMessage.MsgType);
                    break;
            }
        }

        protected override void Close()
        {
            base.Close();

            if (_player != null)
            {
                _app.PlayerController.OnDisconnect(_player);
            }
        }
    }
}
