using CommonLib.Messaging;
using CommonLib.Messaging.Common;
using CommonLib.Messaging.Lobby;
using CommonLib.Server;
using CommonLib.Util;
using LobbyServer.Logic;
using LobbyServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyServer.Test
{
    internal class LoadTest : ITickable
    {
        private static LoadTest _instance;
        public static LoadTest Instance { get => _instance; }
        public static LoadTest Create(AppServer app, int dummyCount = 2000)
        {
            if (_instance == null)
                _instance = new LoadTest(app, dummyCount);

            return _instance;
        }

        public string Name => "Load Test";

        private AppServer _app;
        private Random _rnd;

        private LoadTest(AppServer app, int dummyCount)
        {
            _app = app;

            Setup(dummyCount);
        }

        public void Start()
        {
            _app.Register(this, 1);
        }

        public void Stop()
        {
            _app.Unregister(this);
        }

        public void Setup(int dummyCnt)
        {
            _rnd = new Random(Guid.NewGuid().GetHashCode());
            var dummies = new List<Dummy>();

            for (var i = 0; i < dummyCnt; i++)
                dummies.Add(_app.PlayerController.CreateDummy());

            var dummiesPerRoom = new List<List<Dummy>>();
            var total = 0;
            while (total < dummies.Count)
            {
                var cnt = _rnd.Next(2, 7);
                if (cnt + total >= dummies.Count)
                    cnt = dummies.Count - total;

                if (cnt < 1)
                    break;

                dummiesPerRoom.Add(new List<Dummy>(dummies.GetRange(total, cnt)));
                total += cnt;
            }

            foreach (var dummiesToJoin in dummiesPerRoom)
            {
                var room = _app.RoomController.CreateRoom(dummiesToJoin[0]);

                if (room == null)
                    continue;

                for (var i = 1; i < dummiesToJoin.Count; i++)
                    room.Join(dummiesToJoin[i]);
            }

            CLog.I("Created {0} Dummies spread over {1} rooms", dummyCnt, dummiesPerRoom.Count);
        }

        public void Tick(float delta)
        {
            //Get a list of all rooms
            var rooms = _app.RoomController.ListAllRooms();

            //Remove all rooms from our started list, where the room doesn't exists or isn't plaing any more
            _startedRooms.RemoveWhere(i => rooms.Find(r => r.Index == i && r.IsPlaying) == null);

            //Remove all rooms that we've already started
            rooms.RemoveAll(r => r.IsPlaying || r.PlayerCnt == 1 || !(r.Owner is Dummy) || _startedRooms.Contains(r.Index));

            var rate = rooms.Count / 100f; //Just to have a rate that is proportional to the amount of rooms available;

            int worked = 0;
            foreach (var room in rooms)
            {
                if (_rnd.NextDouble() < rate)
                {
                    StartRoom(room);
                    worked++;
                }

                if (worked > 10)
                    return;
            }
        }

        private HashSet<uint> _startedRooms = new HashSet<uint>();
        private void StartRoom(Room room)
        {
            _startedRooms.Add(room.Index);

            _app.RoomServerController.FindBestServer()?.Session?.Send(new LR_CREATE_ROOM_REQ()
            {
                lobbyRoomIdx = room.Index,
                info = new CREATE_ROOM_INFO()
                {
                    index = room.Index,
                    mapId = room.MapId,
                    name = room.Title,
                    ownerLogin = room.Owner.Session?.Login ?? room.Owner.Nick,
                    maxPlayer = room.MaxPlayer,
                    playerCnt = room.PlayerCnt,
                    stage = room.Stage,
                    hasPassword = (!string.IsNullOrEmpty(room.Password)) ? true : false,
                    slotPlayer = room.ListAllSlots(),
                },
            });
            Thread.Sleep(10); //This is to give time to the message to be sent before trying to send another one.
        }
    }
}
