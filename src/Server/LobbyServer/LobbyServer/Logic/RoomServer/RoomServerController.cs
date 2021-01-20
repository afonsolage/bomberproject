using CommonLib.Util;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyServer.Logic.Server
{
    internal class RoomServerController
    {
        internal const int ROOM_SERVER_INDEX_RANGE = 10000;

        private readonly AppServer _app;
        internal AppServer App => _app;

        private List<RoomServer> _servers;
        private uint _nextUID;

        internal RoomServerController(AppServer app)
        {
            _app = app;

            _servers = new List<RoomServer>();
        }

        internal static uint CalcMinRoomIndex(uint uid)
        {
            return uid * ROOM_SERVER_INDEX_RANGE;
        }

        internal static uint CalcMaxRoomIndex(uint uid)
        {
            return CalcMinRoomIndex(uid + 1) - 1;
        }

        internal void OnConnect(RoomServerSession session)
        {
            RoomServer server = null;
            lock (_servers)
            {
                server = new RoomServer(_nextUID++, session);
                _servers.Add(server);
            }

            session.Setup(this);
        }

        internal void OnDisconnect(RoomServerSession session)
        {
            RoomServer server = null;
            lock (_servers)
            {
                var idx = _servers.FindIndex(r => r.Session.ID == session.ID);
                if (idx < 0)
                {
                    CLog.W("Unable to find room server with session id {0}", session.ID);
                    return;
                }
                server = _servers[idx];
                _servers.RemoveAt(idx);
            }

            _app.RoomController.OnRoomServerDown(server.UID);
        }

        internal RoomServer FindServerByRoomIndex(uint roomIndex)
        {
            return FindServer(roomIndex / ROOM_SERVER_INDEX_RANGE);
        }

        internal RoomServer FindServer(uint id)
        {
            lock (_servers)
            {
                var idx = _servers.FindIndex(r => r.UID == id);
                if (idx >= 0)
                    return _servers[idx];
            }

            return null;
        }

        internal RoomServer FindServer(RoomServerSession session)
        {
            lock (_servers)
            {
                var idx = _servers.FindIndex(r => r.Session.ID == session.ID);
                if (idx >= 0)
                    return _servers[idx];
            }

            return null;
        }

        internal RoomServer FindBestServer()
        {
            lock (_servers)
            {
                //By default the order by is Ascending, so the first one will be the room server with fewer users.
                return _servers.Where(r => r.IsReady && r.UserCount < r.Capacity).OrderBy(r => r.UserCount).FirstOrDefault();
            }
        }

        internal void UpdateServerInfo(uint uid, string ip, int port, int capacity, bool isReady)
        {
            lock (_servers)
            {
                var idx = _servers.FindIndex(r => r.UID == uid);
                if (idx >= 0)
                {
                    _servers[idx].UpdateInfo(ip, port, capacity, isReady);
                }
            }
        }

        internal List<RoomServer> ListAllServers()
        {
            lock (_servers)
            {
                return _servers.Select(s => s).ToList();
            }
        }

        internal void Clear()
        {
            lock (_servers)
            {
                foreach (var server in _servers.ToList())
                {
                    server.Session.Stop();
                }

                _servers.Clear();
            }
        }
    }
}
