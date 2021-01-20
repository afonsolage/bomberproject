using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyServer.Logic.Server
{
    internal class RoomServer
    {
        private readonly uint _uid;
        private readonly RoomServerSession _session;

        internal uint UID => _uid;
        internal RoomServerSession Session => _session;

        internal string IP { get; private set; }
        internal int Port { get; private set; }
        internal int Capacity { get; private set; }

        internal bool IsReady { get; private set; }

        internal int UserCount { get; set; }

        internal RoomServer(uint uid, RoomServerSession session)
        {
            _uid = uid;
            _session = session;
            IsReady = false;
        }

        // This method should be caled only in a thread safe way (prefered in controller inside a lock on server list).
        internal void UpdateInfo(string ip, int port, int capacity, bool isReady)
        {
            IP = ip;
            Port = port;
            Capacity = capacity;

            IsReady = isReady;
        }

        public override bool Equals(object obj)
        {
            return obj is RoomServer server &&
                   _uid == server._uid;
        }

        public override int GetHashCode()
        {
            return 1760909812 + _uid.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[{0}] ({1}:{2}) {3}/{4}", UID, IP, Port, UserCount, Capacity);
        }
    }
}
