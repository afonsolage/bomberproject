#if _DEBUG
using CommonLib.GridEngine;
using RoomServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Networking;
using System.Net.Sockets;
using CommonLib.Util.Math;
using CommonLib.Util;
using RoomServer.Logic.AI;
using RoomServer.Logic.AI.Behaviour;

namespace RoomServer.Logic.Object
{
    class DummySession : ClientSession
    {
        private static uint DUMMY_BASE_ID = 1000;
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
            return true;
        }
    }

    class Dummy : Player
    {
        public Dummy(AppServer app, GridMap map) : base(new DummySession(app), map)
        {
            _behaviour = new PlayerBehaviour(this);
        }

        private string _login;
        public override string Login { get => _login; protected set => _login = value; }

        public void SetLogin(string login)
        {
            _login = login;
            if (_session != null)
                _session.Login = login;

            _session.PlayerInfo = new PlayerInfo()
            {
                nick = _login,
            };
        }

        protected override void OnDead(HitableObject killer)
        {
            base.OnDead(killer);

            //LeaveMap();
        }

        internal override void DestroySessionOnly()
        {
            _session.Player = null;
            _session = null;
        }

        internal override void Reconnect(ClientSession session)
        {
            _session = session;
            _session.Player = this;
            _session.Login = Login;
        }

        internal void Freeze()
        {
            _behaviour = null;
        }

        internal void Unfreeze()
        {
            _behaviour = new PlayerBehaviour(this);
        }
    }
}
#endif
