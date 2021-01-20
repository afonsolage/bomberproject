using CommonLib.Messaging;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Server;
using CommonLib.Util;
using LobbyServer.Logic.Server;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyServer.Logic
{
    internal class Map
    {
        public readonly uint index;
        public readonly string name;
        public readonly ushort width;
        public readonly ushort height;
        public readonly ushort maxPlayer;

        public Map(uint index, string name, ushort width, ushort height, ushort maxPlayer)
        {
            this.index = index;
            this.name = name;
            this.width = width;
            this.height = height;
            this.maxPlayer = maxPlayer;
        }
    }

    internal class RoomController
    {
        private readonly List<Room> _rooms;
        private readonly List<Map> _maps;
        private readonly AppServer _app;
        internal AppServer App { get => _app; }

        public string Name => "RoomController";

        private static int NEXT_ROOM_ID;

        internal void OnRoomStart(Room room, uint roomServerIdx, RoomServer server)
        {
#if _DEBUG
            Debug.Assert(room.Stage != RoomStage.Playing);
#endif

            room.StartMatch(roomServerIdx, server);

            _app.PlayerController.Broadcast(new CL_ROOM_DESTROYED_NFY()
            {
                index = room.Index,
            }/*, PlayerStage.Lobby*/);
        }

        internal void OnRoomFinish(Room room)
        {
#if _DEBUG
            Debug.Assert(room.Stage == RoomStage.Playing);
#endif
            room.FinishMatch();

            _app.PlayerController.Broadcast(new CL_ROOM_CREATED_NFY()
            {
                info = new ROOM_INFO()
                {
                    index = room.Index,
                    mapId = room.MapId,
                    maxPlayer = room.MaxPlayer,
                    name = room.Title,
                    owner = room.Owner.Nick,
                    playerCnt = room.PlayerCnt,
                    stage = room.Stage,
                    password = room.Password,
                    isPublic = (!string.IsNullOrEmpty(room.Password)) ? true : false,

                }
            }/*, PlayerStage.Lobby*/);
        }

        public RoomController(AppServer app)
        {
            // Initialize components.
            _app = app;
            _rooms = new List<Room>();
            _maps = new List<Map>();
        }

        public void AddMap(uint index, string name, ushort width, ushort height, ushort maxPlayer)
        {
            _maps.Add(new Map(index, name, width, height, maxPlayer));
        }

        public Room CreateRoom(Player owner)
        {
            if (_maps.Count == 0)
            {
                CLog.E("Failed to create room: There are no maps loaded.");
                return null;
            }

            var mapIndex = 0; //TODO: Add this to be choosen by client.

            var room = new Room(this, (uint)Interlocked.Increment(ref NEXT_ROOM_ID), owner, _maps[mapIndex].index, _maps[mapIndex].maxPlayer);
            if (!room.Join(owner))
            {
                //_app.PlayerController.Broadcast(new CL_ROOM_CREATE_RES()
                //{
                //    error = MessageError.JOIN_FAIL
                //}, PlayerStage.Lobby);

                return null;
            }

            lock (_rooms)
            {
                _rooms.Add(room);
            }

            _app.PlayerController.Broadcast(new CL_ROOM_CREATED_NFY()
            {
                info = new ROOM_INFO()
                {
                    index = room.Index,
                    mapId = room.MapId,
                    maxPlayer = room.MaxPlayer,
                    name = room.Title,
                    owner = room.Owner.Nick,
                    playerCnt = room.PlayerCnt,
                    stage = room.Stage,
                    password = room.Password,
                    isPublic = (!string.IsNullOrEmpty(room.Password)) ? true : false,
                }
            }/*, PlayerStage.Lobby*/);

            return room;
        }

        internal void OnRoomServerDown(uint id)
        {
            CLog.W("Room server {0} went down. Forcing finish match on all currently playing rooms.", id);

            //Compute the indexes of rooms on this servers
            var minIdx = RoomServerController.CalcMinRoomIndex(id);
            var maxIdx = RoomServerController.CalcMaxRoomIndex(id);

            lock(_rooms)
            {
                var playingRooms = _rooms.FindAll(r => r.IsPlaying && minIdx < r.RoomIndex && r.RoomIndex < maxIdx);

                foreach(var room in playingRooms)
                {
                    room.FinishMatch();
                }
            }
        }

        internal Room FindRoomByOwner(Player player)
        {
            lock (_rooms)
            {
                return _rooms.Find((r) => r.Owner == player);
            }
        }

        public void DestroyRoom(uint index)
        {
            lock (_rooms)
            {
                _rooms.RemoveAll((r) => r.Index == index);
            }

            _app.PlayerController.Broadcast(new CL_ROOM_DESTROYED_NFY()
            {
                index = index
            }/*, PlayerStage.Lobby*/);
        }

        public Room FindRoom(uint index)
        {
            lock (_rooms)
            {
                return _rooms.Find((r) => r.Index == index);
            }
        }

        public Room FindRoomByRoomServerIndex(uint roomServerIndex)
        {
            lock (_rooms)
            {
                return _rooms.Find((r) => r.RoomIndex == roomServerIndex);
            }
        }

        public List<Room> ListAllRooms()
        {
            var res = new List<Room>();

            lock (_rooms)
            {
                foreach (var r in _rooms)
                {
                    res.Add(r.Clone());
                }
            }

            return res;
        }

        internal void RoomUpdated(Room room)
        {
            var info = new ROOM_INFO()
            {
                index = room.Index,
                mapId = room.MapId,
                maxPlayer = room.MaxPlayer,
                name = room.Title,
                owner = room.Owner?.Nick,
                playerCnt = room.PlayerCnt,
                stage = room.Stage,
                password = room.Password,
                isPublic = (!string.IsNullOrEmpty(room.Password)) ? true : false,
            };

            _app.PlayerController.Broadcast(new CL_ROOM_UPDATED_NFY()
            {
                info = info
            });
        }

        internal int Count()
        {
            lock (_rooms)
            {
                return _rooms.Count;
            }
        }
    }
}
