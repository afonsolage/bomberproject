using CommonLib.Messaging;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.Lobby;
using CommonLib.Networking;
using LobbyServer.Logic;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Logic
{
    class DummySession : ClientSession
    {
        private static uint DUMMY_BASE_ID = 1234567;

        public override string Login { get => Player?.Nick ?? _id.ToString(); set => _login = value; }

        public DummySession(AppServer app)
        {
            _app = app;
            _id = DUMMY_BASE_ID++;
        }

        public override bool Send(Packet packet)
        {
            return true;
        }

        public override bool Send<T>(T packet)
        {
            if (Player is Dummy dummy)
                dummy.Handle(packet);

            return true;
        }

        public bool RemoteDisconnect;
        public override bool RemoteDisconnection { get => RemoteDisconnect; }

        protected override void Close()
        {
            return;
        }
    }

    class Dummy : Player
    {
        private static uint DUMMY_BASE_ID = 1000;
        private AppServer _app;

        public Dummy(AppServer app) : base(++DUMMY_BASE_ID, "Dummy " + DUMMY_BASE_ID, PlayerGender.None, 99, 0, new DummySession(app), PlayerStage.Lobby)
        {
            _app = app;
            _session.Player = this;

            _isDummy = true;

            // Dummy has no ping, so ping will always be 1.
            Ping = 1;

            // Random sex of dummy.
            Random random = new Random();
            _gender = (PlayerGender)random.Next(1, 3);
        }

        public override string ToString()
        {
            return string.Format("[{0}]{1} {2} ({3}{4})", Index, Nick, Stage, Room?.Index, (Room != null && Ready) ? " Ready" : "");
        }

        internal void Handle<T>(T packet) where T : IMessage
        {
            switch(packet.MsgType)
            {
                case MessageType.CL_ROOM_START_NFY:
                    OnRoomStart(packet as CL_ROOM_START_NFY);
                    break;
            }
        }

        private void OnRoomStart(CL_ROOM_START_NFY nfy)
        {
            var roomServer = _session.App.RoomServerController.FindServerByRoomIndex(nfy.roomIndex);
#if _DEBUG
            Debug.Assert(roomServer != null);
#endif
            // When the match starts for a dummy, notify the room server to create a dummy matching this one and disconnects it.
            roomServer?.Session?.Send(new LR_DUMMY_JOIN_NFY()
            {
                login = _session.Login,
                dummyIndex = (uint)_index,
                roomIndex = nfy.roomIndex,
            });

            _session = null;
        }

        internal override void OnFinishMatch()
        {
            base.OnFinishMatch();

            // When the match finishes for a dummy, just reconnects him.
            _session = new DummySession(_app)
            {
                Player = this
            };

            TimeoutTicks = 0;
        }
    }
}