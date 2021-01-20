using System;
using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Networking;
using CommonLib.Util;
using RoomServer.Server.Messaging;
using RoomServer.Logic.Object;
using System.Collections.Generic;

namespace RoomServer.Server
{
    class PlayerInfo
    {
        public ulong index;
        public string nick;
        public PlayerGender gender;
        public PlayerStage stage;
        public PlayerState state;
        public byte privilege;
        public bool firstLogin;
        public uint roomIndex;
        public int roomSlotIndex;
        public long ping;
    }

    class ClientSession : ClientConnection
    {
        private bool _authenticated;
        public bool Authenticated { get => _authenticated; set => _authenticated = value; }

        private ulong _dbid;
        public ulong DBID { get => _dbid; set => _dbid = value; }

        private string _login;
        public string Login { get => _login; set => _login = value; }

        private string _token;
        public string Token { get => _token; set => _token = value; }

        protected AppServer _app;
        public AppServer App { get => _app; }

        protected Player _player;
        public Player Player { get => _player; set => _player = value; }

        public PlayerInfo PlayerInfo { get; set; }

        public virtual bool RemoteDisconnection { get => !_closeRequested; }

        public void Setup(AppServer app)
        {
            _app = app;
            _authenticated = false;

            Ready();

            SendWelcomeMessage();
        }

        protected void SendWelcomeMessage()
        {
            Send(new CR_WELCOME_NFY()
            {
                serverName = _app.Name,
                uid = ID
            });
        }

        public override void Handle(Packet packet)
        {
            var rawMessage = new RawMessage(packet.buffer);

            CLog.D("Received packet {0} from {1}", rawMessage.MsgType, _socket.RemoteEndPoint);

            if (!_authenticated && rawMessage.MsgType != MessageType.CX_TOKEN_REQ)
            {
                CLog.W("Received message without beign authenticated first. Closing connection.");
                Close();
                return;
            }

            switch (rawMessage.MsgType)
            {
                case MessageType.CX_TOKEN_REQ:
                    ClientMessages.TokenReq(rawMessage.To<CX_TOKEN_REQ>(), this);
                    break;
                case MessageType.CR_JOIN_ROOM_REQ:
                    ClientMessages.JoinRoomReq(rawMessage.To<CR_JOIN_ROOM_REQ>(), this);
                    break;
                default:
                    ClientMessages.RoomMessage(rawMessage, this);
                    break;
            }
        }

        protected override void Close()
        {
            base.Close();

            if (RemoteDisconnection)
            {
                _player?.DestroySessionOnly();
            }
            else
            {
                _player?.LeaveMap();
            }
        }
    }
}
