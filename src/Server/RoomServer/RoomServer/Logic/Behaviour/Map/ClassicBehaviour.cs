using CommonLib.GridEngine;
using CommonLib.Util.Math;
using CommonLib.Messaging.Client;
using CommonLib.Messaging.Common;
using LightJson;
using RoomServer.Logic.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomServer.Logic.Behaviour.Map
{
    class ClassicBehaviour : RoomBehaviour
    {
        private float _elapsedTime = 0;
        private float _hurryupTime;
        private List<Vec2> _loop;

        internal override void Setup(params object[] args)
        {
            base.Setup(args);

            var settings = args[1] as JsonObject;

            _hurryupTime = (float)settings["hurryupTime"].AsNumber;
        }

        internal override void OnInit()
        {
            _loop = new List<Vec2>();
            var cnt = _room.CellData.Length;

            var idx = 0;
            var current = new Vec2(0, 0);
            var add = Vec2.ALL_DIRS[idx++ % Vec2.ALL_DIRS.Length];
            _loop.Add(current);

            while (_loop.Count < cnt)
            {
                var newPos = current + add;

                if (newPos.IsOnBounds(_room.MapSize.x, _room.MapSize.y) && !_loop.Contains(newPos))
                {
                    current += add;
                    _loop.Add(current);
                }
                else
                {
                    add = Vec2.ALL_DIRS[idx++ % Vec2.ALL_DIRS.Length];
                }
            }

            GenerateTerrain();
        }

        private void GenerateTerrain()
        {
            var excludedArea = new List<Vec2>();

            foreach (var spawn in _room.SpawnPlaces)
            {
                excludedArea.Add(spawn);

                foreach (var dir in Vec2.ALL_DIRS)
                {
                    var pos = spawn + dir;
                    var cell = _room[pos.x, pos.y];

                    if (cell == null)
                        continue;

                    excludedArea.Add(pos);
                }
            }

            for (var x = 0; x < _room.MapSize.x; x++)
            {
                for (var y = 0; y < _room.MapSize.y; y++)
                {
                    var pos = new Vec2(x, y);
                    var cell = _room[pos.x, pos.y];

                    if (cell == null)
                        continue;

                    if (excludedArea.Contains(pos))
                        continue;

                    if (cell.Type != CellType.None)
                        continue;

                    if (cell.FindAllByType<GridObject>()?.Count > 0)
                        continue;

                    cell.UpdateCell(GenerateCell(pos));
                }
            }
        }

        private Random _genCellRand = new Random((int)DateTime.Now.Ticks);
        private CellType GenerateCell(Vec2 pos)
        {
            var rand = _genCellRand.Next(0, 100);

            if (rand < 30)
                return CellType.None;
            else if (rand < 80)
                return CellType.Plant;
            else
                return CellType.Wooden;
        }

        private float hurryUpTickInterval = 0.5f;
        private float nextHurryUpTick = 0;
        internal override void OnTick(float delta)
        {
            //Just count hurry up time when room is initialized
            if (!_room.IsInitialized)
                return;

            _elapsedTime += delta;

            if (_elapsedTime > _hurryupTime)
            {
                if (nextHurryUpTick <= _elapsedTime)
                    HurryUp();
            }

            CheckEndGame();
        }

        public Dictionary<Vec2, long> GetHurryUpPlaces()
        {
            var res = new Dictionary<Vec2, long>();
            var i = 0;
            var hurryUpTime = (long)(_hurryupTime * 1000);

            foreach(var pos in _loop)
            {
                res[pos] = hurryUpTime + (long)(i++ * hurryUpTickInterval * 1000);
            }

            return res;
        }

        private void HurryUp()
        {
            nextHurryUpTick = _elapsedTime + hurryUpTickInterval;

            foreach (var pos in _loop)
            {
                var cell = _room[pos.x, pos.y];

                if (cell == null || cell.Type == CellType.Anvil || cell.Type == CellType.Invisible)
                    continue;

                var diedObject = cell.FindAllByType<HitableObject>();

                foreach (var obj in diedObject)
                {
                    var hittable = obj as HitableObject;
                    hittable.HitKill();
                }

                cell.UpdateCell(CellType.Anvil);
                _room.BroadcastMessage(new CR_HURRY_UP_CELL_NFY()
                {
                    cell = new VEC2()
                    {
                        x = pos.x,
                        y = pos.y,
                    },
                    replaceType = CellType.Anvil, //TODO: Add config for it.
                });

                return;
            }
        }

        private void CheckEndGame()
        {
            var liveCnt = 0;
            var playerCnt = 0;
            Player lastAlive = null;

            foreach (var obj in _room.GetActiveObjects())
            {
                if (obj.Type != ObjectType.PLAYER)
                    continue;

                playerCnt++;
                var player = obj as Player;

                if (player.IsLive)
                {
                    liveCnt++;
                    lastAlive = player;
                }
            }

            if (liveCnt <= 1)
            {
                _room.EndMatch(new MatchEndInfo()
                {
                    winner = lastAlive?.UID ?? 0,
                });
            }
        }

        internal override void Destroy()
        {
        }
    }
}
