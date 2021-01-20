using CommonLib.GridEngine;
using CommonLib.Messaging.Base;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.DB;
using CommonLib.Messaging.Lobby;
using CommonLib.Util;
using LightJson;
using LightJson.Serialization;
using RoomServer.Logic.Behaviour.Map;
using RoomServer.Server;
using RoomServer.Server.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic
{
    internal class MapInfo
    {
        public uint index;
        public string name;
        public ushort width;
        public ushort height;
        public ushort playerCnt;
        public uint background;
        public byte[] data;
        public Dictionary<string, JsonObject> behaviour;
    }

    internal class RoomManager : GridManager<Room>
    {
        internal const int LOBBY_SERVER_INDEX_RANGE = 10000;
        private static readonly string ROOM_BEHAVIOUR_NAMESPACE = "RoomServer.Logic.Behaviour.Map";

        private readonly AppServer _app;
        public AppServer App
        {
            get
            {
                return _app;
            }
        }

        private readonly List<MapInfo> _baseMapList;

        public RoomManager(AppServer app)
        {
            _app = app;
            _baseMapList = new List<MapInfo>();
        }

        public void Init()
        {
            _app.DBClient.Send(new DR_STARTUP_INFO_REQ());
        }

        internal void DestroyRoom(Room room)
        {
            CLog.D("Destroyed room: {0}", room);
            _app.Unregister(room);
            Destroy(room);
        }

        internal Room CreateRoom(uint mapId, string ownerLogin, uint botCount = 0)
        {
            var baseInfo = _baseMapList.Find(info => info.index == mapId);

            if (baseInfo == null)
            {
                CLog.W("Unable to find base map: {0}. Room creation failed.", mapId);
                return null;
            }

            var room = Create(0, baseInfo.width, baseInfo.height, baseInfo.data);
            room.BotCount = botCount;
            room.MapId = mapId;
            room.OwnerLogin = ownerLogin;

            foreach (var pair in baseInfo.behaviour)
            {
                room.Behaviour.AddBehaviour(string.Format("{0}.{1}Behaviour", ROOM_BEHAVIOUR_NAMESPACE, pair.Key), pair.Value);
            }
            //room.Behaviour.AddBehaviour<DebugBehaviour>();

            _app.Register(room, Room.TICKS_PER_SECOND);

            CLog.D("Created room: {0}", room);

            return room;
        }

        internal void ForceFinishAllRooms()
        {
            lock(_maps)
            {
                foreach(var pair in _maps)
                {
                    var room = pair.Value as Room;
                    room.RequestShutdown();
                }
            }
        }

        internal void SetBaseID(uint baseId)
        {
            _nextMapUid = baseId * LOBBY_SERVER_INDEX_RANGE + 1;
        }

        protected override Room Instanciate(float cellSize, ushort width, ushort height, uint uid)
        {
            return new Room(this, width, height, uid);
        }

        public void Init(List<Tuple<int, int>> typeList)
        {
            var configList = new List<CellConfig>(typeList.Count);

            foreach (Tuple<int, int> t in typeList)
            {
                configList.Add(new CellConfig(t.Item1, t.Item2));
            }

            base.Init(configList);

            if (_baseMapList.Count > 0)
                _app.RoomManagerReady = true;
        }

        internal void LoadMaps(List<MAP_INFO> mapList)
        {
            foreach (var info in mapList)
            {
                var newInfo = new MapInfo()
                {
                    index = info.index,
                    name = info.name,
                    width = info.width,
                    height = info.height,
                    playerCnt = info.playerCnt,
                    background = info.background,
                    data = info.data,
                    behaviour = new Dictionary<string, JsonObject>(),
                };

                foreach (var pair in info.behaviour)
                {
                    JsonObject val = (string.IsNullOrEmpty(pair.Value)) ? null : JsonReader.Parse(pair.Value).AsJsonObject;
                    newInfo.behaviour.Add(pair.Key, val);
                }

                _baseMapList.Add(newInfo);
            }

            if (_initialized)
                _app.RoomManagerReady = true;
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
        }

        public void AddRoomMessage(uint room, SessionMessage message)
        {
            var map = Find(room);

            if (map == null)
            {
                CLog.W("Failed to add message to room. Can't find a room with uid: {0}", room);
                return;
            }
            else
            {
                map.AddMessage(message);
            }
        }

        public uint GetBackground(uint mapId)
        {
            var baseInfo = _baseMapList.Find(info => info.index == mapId);

            if (baseInfo == null)
            {
                CLog.W("Unable to find base map: {0}. Room creation failed.", mapId);
                return 0;
            }

            return baseInfo.background;
        }

        public void PrintRooms()
        {
            lock (_maps)
            {
                if (_maps.Count == 0)
                {
                    CLog.I("There are no rooms created yet.");
                }
                else
                {

                    foreach (var room in _maps)
                    {
                        CLog.I("Room {0} owner: {1}\n", room.Value.UID, room.Value.Owner);
                    }
                }
            }
        }

        internal void MatchEnded(Room room, MatchEndInfo matchEndInfo)
        {
            App.LobbyClient.Send(new LR_ROOM_FINISHED_NFY()
            {
                index = room.UID,
                info = new MATCH_INFO()
                {
                    winner = matchEndInfo.winner,
                }
            });
        }

        internal int PlayerCount()
        {
            lock(_maps)
            {
                //Since we are getting objects from a room, that is constantly changing, we need to check if the returned object isnt null.
                return _maps.Values.Select(r => r.ActivePlayersCount).Sum();
            }
        }

        internal int Count()
        {
            lock(_maps)
            {
                return _maps.Values.Count;
            }
        }
    }
}
